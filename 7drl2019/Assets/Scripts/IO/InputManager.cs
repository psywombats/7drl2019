using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour {

    public enum Command {
        Left,
        Right,
        Up,
        Down,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        Wait,
        Confirm,
        Cancel,
        Menu,
        Click,
        Rightclick,
        Debug,
        Examine,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Skill5,
        Skill6,
    };

    public enum Event {
        Down,
        Up,
        Hold,
        Repeat,
    };

    private static readonly float KeyRepeatSeconds = 0.5f;

    private Dictionary<Command, List<KeyCode>> keybinds;
    private List<InputListener> listeners;
    private List<InputListener> disabledListeners;
    private Dictionary<Command, float> holdStartTimes;
    private List<KeyCode> fastKeys;
    private Dictionary<string, InputListener> anonymousListeners;
    private bool simulatedAdvance;

    public void Awake() {
        keybinds = new Dictionary<Command, List<KeyCode>>();
        keybinds[Command.Left] = new List<KeyCode>(new[] { KeyCode.LeftArrow, KeyCode.Keypad4, KeyCode.Alpha4 });
        keybinds[Command.UpLeft] = new List<KeyCode>(new[] { KeyCode.Keypad7, KeyCode.Alpha7 });
        keybinds[Command.Up] = new List<KeyCode>(new[] { KeyCode.UpArrow, KeyCode.Keypad8, KeyCode.Alpha8 });
        keybinds[Command.UpRight] = new List<KeyCode>(new[] { KeyCode.Keypad9, KeyCode.Alpha9 });
        keybinds[Command.Right] = new List<KeyCode>(new[] { KeyCode.RightArrow,KeyCode.Keypad6, KeyCode.Alpha6 });
        keybinds[Command.DownRight] = new List<KeyCode>(new[] { KeyCode.Keypad3, KeyCode.Alpha3 });
        keybinds[Command.Down] = new List<KeyCode>(new[] { KeyCode.DownArrow, KeyCode.Keypad2, KeyCode.Alpha2 });
        keybinds[Command.DownLeft] = new List<KeyCode>(new[] { KeyCode.Keypad1, KeyCode.Alpha1 });
        keybinds[Command.Wait] = new List<KeyCode>(new[] { KeyCode.Keypad5, KeyCode.Period, KeyCode.Alpha5 });
        keybinds[Command.Confirm] = new List<KeyCode>(new[] { KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter });
        keybinds[Command.Cancel] = new List<KeyCode>(new[] { KeyCode.Escape, KeyCode.Backspace });
        keybinds[Command.Debug] = new List<KeyCode>(new[] { KeyCode.Tilde });
        keybinds[Command.Menu] = new List<KeyCode>(new[] { KeyCode.Escape, KeyCode.C, KeyCode.Backspace });
        keybinds[Command.Click] = new List<KeyCode>();
        keybinds[Command.Rightclick] = new List<KeyCode>();
        keybinds[Command.Examine] = new List<KeyCode>(new[] { KeyCode.E, KeyCode.V, KeyCode.X });
        keybinds[Command.Skill1] = new List<KeyCode>(new[] { KeyCode.F1 });
        keybinds[Command.Skill2] = new List<KeyCode>(new[] { KeyCode.F2 });
        keybinds[Command.Skill3] = new List<KeyCode>(new[] { KeyCode.F3 });
        keybinds[Command.Skill4] = new List<KeyCode>(new[] { KeyCode.F4 });
        keybinds[Command.Skill5] = new List<KeyCode>(new[] { KeyCode.F5 });
        keybinds[Command.Skill6] = new List<KeyCode>(new[] { KeyCode.F6 });
        fastKeys = new List<KeyCode>(new[] { KeyCode.LeftControl, KeyCode.RightControl });

        listeners = new List<InputListener>();
        disabledListeners = new List<InputListener>();

        listeners = new List<InputListener>();
        holdStartTimes = new Dictionary<Command, float>();

        anonymousListeners = new Dictionary<string, InputListener>();
    }

    public void Update() {
        List<InputListener> listeners = new List<InputListener>();
        listeners.AddRange(this.listeners);

        foreach (InputListener listener in listeners) {
            if (disabledListeners.Contains(listener)) {
                continue;
            }

            bool endProcessing = false; // ew.
            foreach (Command command in System.Enum.GetValues(typeof(Command))) {
                foreach (KeyCode code in keybinds[command]) {
                    if (Input.GetKeyDown(code)) {
                        endProcessing |= listener.OnCommand(command, Event.Down);
                    }
                    if (Input.GetKeyUp(code)) {
                        endProcessing |= listener.OnCommand(command, Event.Up);
                        holdStartTimes.Remove(command);
                    }
                    if (Input.GetKey(code)) {
                        if (!holdStartTimes.ContainsKey(command)) {
                            holdStartTimes[command] = Time.time;
                        }
                        endProcessing |= listener.OnCommand(command, Event.Hold);
                        if (Time.time - holdStartTimes[command] > KeyRepeatSeconds) {
                            endProcessing |= listener.OnCommand(command, Event.Repeat);
                        }
                    }
                    if (endProcessing) break;
                }
                if (endProcessing) break;
            }
            if (endProcessing) break;
        }
    }

    public void PushListener(string id, Func<Command, Event, bool> responder) {
        InputListener listener = new AnonymousListener(responder);
        anonymousListeners[id] = listener;
        PushListener(listener);
    }
    public void PushListener(InputListener listener) {
        listeners.Insert(0, listener);
    }

    public void RemoveListener(string id) {
        listeners.Remove(anonymousListeners[id]);
    }
    public void RemoveListener(InputListener listener) {
        listeners.Remove(listener);
    }

    public void DisableListener(InputListener listener) {
        disabledListeners.Add(listener);
    }

    public void EnableListener(InputListener listener) {
        if (disabledListeners.Contains(listener)) {
            disabledListeners.Remove(listener);
        }
    }

    public bool IsFastKeyDown() {
        foreach (KeyCode code in fastKeys) {
            if (Input.GetKey(code)) {
                return true;
            }
        }
        return false;
    }

    public int CommandToNumber(Command cmd) {
        switch (cmd) {
            case Command.Skill1: return 1;
            case Command.Skill2: return 2;
            case Command.Skill3: return 3;
            case Command.Skill4: return 4;
            case Command.Skill5: return 5;
            case Command.Skill6: return 6;
        }
        Debug.Assert(false);
        return -1;
    }

    // simulates the user pushing a command
    // called by input listeners usually when interpreting clicks as answers to AwaitAdvance
    public void SimulateCommand(Command simulatedCommand) {
        simulatedAdvance = true;
        InputListener listener = listeners[listeners.Count - 1];
        if (!disabledListeners.Contains(listener)) {
            listener.OnCommand(simulatedCommand, Event.Down);
            listener.OnCommand(simulatedCommand, Event.Up);
        }
    }

    public IEnumerator AwaitConfirm() {
        bool advance = false;
        simulatedAdvance = false;
        while (advance == false) {
            foreach (KeyCode code in keybinds[Command.Confirm]) {
                if (Input.GetKeyDown(code)) {
                    advance = true;
                }
            }
            if (simulatedAdvance) {
                advance = true;
            }
            yield return null;
        }
        simulatedAdvance = false;
    }
}
