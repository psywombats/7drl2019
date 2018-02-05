using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class SceneScript {

    private const string ScenesDirectory = "SceneScripts";

    // parsing state
    private ChoiceCommand choice;
    private StageDirectionCommand lastStageDirection;
    private BranchCommand lastBranch;
    private ExitAllCommand lastExitAll;
    private static bool holdMode;
    private bool nvlMode;

    // playback state
    private List<SceneCommand> commands;
    private string sceneName;
    private int commandIndex;

    public SceneCommand CurrentCommand { get; set; }

    public SceneScript(ScenePlayer player, TextAsset asset) {
        sceneName = asset.name;
        ParseCommands(player, asset.text);
        commandIndex = 0;
    }

    public SceneScript(ScenePlayer player, ScreenMemory memory) : this(player, AssetForSceneName(memory.sceneName)) {
        commandIndex = memory.commandNumber;
    }

    public static TextAsset AssetForSceneName(string sceneName) {
        return Resources.Load<TextAsset>(ScenesDirectory + "/" + sceneName);
    }

    public static bool StartsWithName(string text) {
        if ((text.IndexOf(' ') == -1) || (text.IndexOf(':') == -1)) {
            return false;
        }
        foreach (char c in text) {
            if (c == ':') {
                return true;
            }
            if (!Char.IsUpper(c)) {
                return false;
            }
        }
        return false;
    }

    public IEnumerator PerformActions(ScenePlayer player) {
        for (; commandIndex < commands.Count; commandIndex += 1) {
            CurrentCommand = commands[commandIndex];
            while (player.IsSuspended()) {
                yield return null;
            }
            if (!Global.Instance().memory.HasSeenCommand(sceneName, commandIndex)) {
                player.SkipMode = false;
            }
            if (player.debugBox != null) {
                player.debugBox.text = "scene: " + sceneName + "\n";
                player.debugBox.text += "command index: " + commandIndex;
            }
            yield return player.StartCoroutine(CurrentCommand.PerformAction(player));
            Global.Instance().memory.AcknowledgeCommand(sceneName, commandIndex);
        }
    }

    public bool ShouldUseFastMode(ScenePlayer player) {
        if (Global.Instance().input.IsFastKeyDown()) {
            return true;
        }
        bool hasSeenCommand = Global.Instance().memory.HasSeenCommand(sceneName, commandIndex);
        Setting<bool> skipUnreadSetting = Global.Instance().settings.GetBoolSetting(SettingsConstants.SkipUnreadText);
        bool skipUnread;
        if (skipUnreadSetting == null) {
            skipUnread = false;
        } else {
            skipUnread = skipUnreadSetting.Value;
        }
        if (player.SkipMode && (hasSeenCommand || skipUnread)) {
            return true;
        }
        return false;
    }

    public bool CanUseFastMode() {
        bool hasSeenCommand = Global.Instance().memory.HasSeenCommand(sceneName, commandIndex);
        Setting<bool> skipUnreadSetting = Global.Instance().settings.GetBoolSetting(SettingsConstants.SkipUnreadText);
        bool skipUnread;
        if (skipUnreadSetting == null) {
            skipUnread = false;
        } else {
            skipUnread = skipUnreadSetting.Value;
        }
        return hasSeenCommand || skipUnread;
    }

    public void PopulateMemory(ScreenMemory memory) {
        memory.commandNumber = commandIndex;
        memory.sceneName = sceneName;
    }
    
    private void ParseCommands(ScenePlayer player, string text) {
        choice = null;
        commands = new List<SceneCommand>();
        string[] commandStrings = text.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None);
        bool startsNewParagraph = true;
        foreach (string commandString in commandStrings) {
            SceneCommand command;

            if (player!= null && player.debugBox != null) {
                player.debugBox.text = "parsing scene: " + sceneName + "\n";
                player.debugBox.text += "command string: " + commandString;
            }

            if (commandString.Trim().Length == 0) {
                // newline, this is a command to clear the stage of characters presuming no HOLDs

                if (!holdMode) {
                    this.lastExitAll = new ExitAllCommand();
                    command = this.lastExitAll;
                    if (lastStageDirection != null) {
                        lastStageDirection.SetSynchronous();
                    }
                } else {
                    command = null;
                }
                startsNewParagraph = true;
            } else if (commandString[0] == '[') {
                // this is a command of some type

                startsNewParagraph = false;
                if (commandString.IndexOf(']') == commandString.Length - 1) {
                    // infix command
                    if (commandString.IndexOf(' ') == -1) {
                        // single word command
                        command = ParseCommand(commandString.Substring(1, commandString.Length - 2), new List<string>());
                    } else {
                        // multiword infix command
                        string keyword = commandString.Substring(1, commandString.IndexOf(' ') - 1);
                        string argsString = commandString.Substring(commandString.IndexOf(' ') + 1,
                                commandString.Length - (keyword.Length + 3));
                        string[] args = argsString.Split();
                        command = ParseCommand(keyword, new List<string>(args));
                    }
                } else {
                    // postfix command
                    string keyword = commandString.Substring(1, commandString.IndexOf(' ') - 1);
                    string argsString = commandString.Substring(keyword.Length + 2, commandString.IndexOf(']') - (keyword.Length + 2));
                    string[] args = argsString.Split();
                    string content = commandString.Substring(commandString.IndexOf(']') + 2);
                    command = ParseCommand(keyword, new List<string>(args), content);
                }
            } else {
                // this is a text literal
                
                if (StartsWithName(commandString)) {
                    // spoken line
                    command = ParseLine(player, commandString);
                } else {
                    if (startsNewParagraph) {
                        // text paragraph
                        command = ParseParagraph(commandString);
                    } else {
                        // the inner monologue
                        command = ParseLine(player, commandString);
                    }
                }
                startsNewParagraph = false;
                holdMode = false;
                if (lastStageDirection != null) {
                    lastStageDirection.SetSynchronous();
                }
            }

            if (command != null) {
                commands.Add(command);
            }
        }
    }

    private SceneCommand ParseCommand(string command, List<string> args, string text = "") {
        switch (command) {
            case "goto":
                return new GotoCommand(args[0]);
            case "choice":
                this.choice = new ChoiceCommand();
                return this.choice;
            case "enter":
                this.lastStageDirection = new EnterCommand(args[0], args[1], OptionalArg(args, 2));
                return this.lastStageDirection;
            case "exit":
                this.lastStageDirection = new ExitCommand(args[0], OptionalArg(args, 1));
                return this.lastStageDirection;
            case "clear":
                this.lastStageDirection = new ExitAllCommand();
                return this.lastStageDirection;
            case "hold":
                // don't bother an explicit command here, this is really just a meta-command about parsing
                holdMode = true;
                return null;
            case "increment":
                return new IncrementCommand(args[0], 1);
            case "decrement":
                return new IncrementCommand(args[0], -1);
            case "branch":
                this.lastBranch = new BranchCommand(args[0], args[1], args[2]);
                return this.lastBranch;
            case "true":
                this.lastBranch.TrueSceneName = args[1];
                return null;
            case "false":
                this.lastBranch.FalseSceneName = args[1];
                return null;
            case "end":
                return new EndCommand(args[0]);
            case "perspective":
                return new PerspectiveCommand(args[0], OptionalArg(args, 1), text);
            case "bg":
                return new BackgroundCommand(args[0], OptionalArg(args, 1));
            case "bgm":
                return new BGMCommand(args[0]);
            case "sfx":
                return new SoundEffectCommand(args[0]);
            case "wait":
                return new WaitCommand(args[0]);
            default:
                if (choice != null) {
                    string choiceString = command + " " + String.Join(" ", args.ToArray());
                    this.choice.AddOption(new ChoiceOption(choiceString));
                }
                //Assert.IsTrue(false, "bad command: " + command);
                return null;
        }
    }

    private string OptionalArg(List<String> args, int index) {
        return (index < args.Count) ? args[index] : null;
    }

    private SceneCommand ParseLine(ScenePlayer player, string commandString) {
        if (nvlMode) {
            if (lastExitAll != null) {
                lastExitAll.ClosesTextboxes = true;
                lastExitAll = null;
            }
        }
        nvlMode = false;
        return new SpokenLineCommand(player, commandString);
    }

    private SceneCommand ParseParagraph(string commandString) {
        if (!nvlMode) {
            if (lastExitAll != null) {
                lastExitAll.ClosesTextboxes = true;
                lastExitAll = null;
            }
        }
        nvlMode = true;
        return new ParagraphCommand(commandString);
    }
}
