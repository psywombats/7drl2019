using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using MoonSharp.Interpreter;

public class ScenePlayer : MonoBehaviour, InputListener {

    private const string DialogSceneName = "DialogScene";
    private const float hiddenTextModeFadeoutSeconds = 0.6f;

    public TextAsset firstSceneFile;
    public Canvas canvas;
    public TextboxComponent textbox;
    public TextboxComponent paragraphBox;
    public BackgroundComponent background;
    public PortraitGroupComponent portraits;
    public TransitionComponent transition;
    public UnityEngine.UI.Text debugBox;

    public TransitionIndexData transitions;
    public FadeIndexData fades;

    private SceneScript currentScript;
    private IEnumerator playingRoutine;
    private bool suspended;
    private bool wasHurried;
    private bool hiddenTextMode;

    public bool AwaitingInputFromCommand { get; set; }
    public bool SkipMode { get; set; }
    public bool AutoMode { get; set; }

    public static void LoadScreen() {
        SceneManager.LoadScene(DialogSceneName);
    }

    public void Start() {
        //textbox.gameObject.SetActive(false);
        //paragraphBox.gameObject.SetActive(false);
        
        //Global.Instance().Input.PushListener(this);
        //Global.Instance().Lua.SetGlobal("player", this);
        
        //portraits.HideAll();

        //StartCoroutine(CoUtils.RunAfterDelay(0.1f, () => {
        //    if (Global.Instance().Memory.ActiveMemory != null) {
        //        Global.Instance().Memory.PopulateFromMemory(Global.Instance().Memory.ActiveMemory);
        //        Global.Instance().Memory.ActiveMemory = null;
        //        ResumeLoadedScene();
        //    } else {
        //        PlayFirstScene();
        //    }
        //}));
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Confirm:
                if (hiddenTextMode) {
                    SetHiddenTextMode(false);
                } else if (AutoMode) {
                    AutoMode = false;
                } else {
                    wasHurried = true;
                }
                return true;
            case InputManager.Command.Menu:
                StartCoroutine(PauseRoutine());
                return true;
            case InputManager.Command.Skip:
                SkipMode = !SkipMode;
                return true;
            case InputManager.Command.Auto:
                AutoMode = !AutoMode;
                return true;
            case InputManager.Command.Click:
                if (hiddenTextMode) {
                    SetHiddenTextMode(false);
                } else {
                    Global.Instance().Input.SimulateCommand(InputManager.Command.Confirm);
                }
                return true;
            case InputManager.Command.Rightclick:
                SetHiddenTextMode(!hiddenTextMode);
                return true;
            case InputManager.Command.Log:
                StartCoroutine(LogRoutine());
                return true;
            case InputManager.Command.Save:
                Global.Instance().Memory.RememberScreenshot();
                StartCoroutine(SaveRoutine());
                return true;
            case InputManager.Command.Load:
                StartCoroutine(LoadRoutine());
                return true;
            default:
                return false;
        }
    }

    public void SetInputEnabled(bool enabled) {
        if (enabled) {
            currentScript.CurrentCommand.OnFocusGained();
        } else {
            currentScript.CurrentCommand.OnFocusGained();
        }
    }

    public bool WasHurried() {
        return wasHurried;
    }

    public bool IsSuspended() {
        return suspended;
    }

    public bool IsAutoAvailable() {
        // this may change in the future
        return true;
    }

    public bool IsSkipAvailable() {
        if (currentScript == null) {
            return false;
        } else {
            return currentScript.CanUseFastMode();
        }
    }

    public bool ShouldUseFastMode() {
        if (currentScript == null) {
            return false;
        } else {
            return currentScript.ShouldUseFastMode(this);
        }
    }

    public void AcknowledgeHurried() {
        wasHurried = false;
    }

    public void PlayFirstScene() {
        StartCoroutine(PlayScriptForScene(firstSceneFile));
    }

    public IEnumerator AwaitHurry() {
        while (!WasHurried() && !ShouldUseFastMode() && !AutoMode) {
            yield return null;
        }
        AcknowledgeHurried();
    }

    public FadeComponent GetFade() {
        return FindObjectOfType<FadeComponent>();
    }
    
    public SpriteEffectComponent GetEffect() {
        return FindObjectOfType<SpriteEffectComponent>();
    }

    public IEnumerator PlayScriptForScene(string sceneName) {
        TextAsset file = SceneScript.AssetForSceneName(sceneName);
        yield return StartCoroutine(PlayScriptForScene(file));
    }

    public IEnumerator PlayScriptForScene(TextAsset sceneFile) {
        currentScript = new SceneScript(this, sceneFile);
        yield return StartCoroutine(PlayCurrentScript());
    }

    public void ResumeLoadedScene() {
        StartCoroutine(PlayCurrentScript());
    }

    public ScreenMemory ToMemory() {
        ScreenMemory memory = new ScreenMemory();
        currentScript.PopulateMemory(memory);
        if (AwaitingInputFromCommand) {
            memory.commandNumber -= 1;
        }
        portraits.PopulateMemory(memory);
        background.PopuateMemory(memory);
        return memory;
    }

    public void PopulateFromMemory(ScreenMemory memory) {
        if (playingRoutine != null) {
            StopCoroutine(playingRoutine);
        }
        currentScript = new SceneScript(this, memory);
        portraits.PopulateFromMemory(memory);
        background.PopulateFromMemory(memory);
    }

    public IEnumerator ResumeRoutine() {
        yield return CoUtils.RunParallel(new[] {
            textbox.FadeInRoutine(this, PauseMenuComponent.FadeoutSeconds),
            paragraphBox.FadeInRoutine(this, PauseMenuComponent.FadeoutSeconds)
        }, this);
        suspended = false;
    }

    public IEnumerator ExecuteTransition(string tag, Action intermediate) {
        TransitionData data = transitions.GetData(tag);
        StartCoroutine(transition.TransitionRoutine(data, intermediate));
        while (transition.IsTransitioning()) {
            if (ShouldUseFastMode()) {
                transition.Hurry();
            }
            yield return null;
        }
    }

    private void SetHiddenTextMode(bool hidden) {
        StartCoroutine(SetHiddenTextModeRoutine(hidden));
    }

    private IEnumerator PauseRoutine() {
        Global.Instance().Memory.RememberScreenshot();
        yield return StartCoroutine(DisplayMenu(PauseMenuComponent.Spawn(canvas.gameObject, () => {
            StartCoroutine(ResumeRoutine());
        })));
    }

    private IEnumerator LogRoutine() {
        yield return StartCoroutine(DisplayMenu(LogComponent.Spawn(canvas.gameObject, () => {
            StartCoroutine(ResumeRoutine());
        })));
    }

    private IEnumerator SaveRoutine() {
        yield return StartCoroutine(DisplayMenu(SaveMenuComponent.Spawn(canvas.gameObject, SaveMenuComponent.SaveMenuMode.Save, () => {
            StartCoroutine(ResumeRoutine());
        })));
    }

    private IEnumerator LoadRoutine() {
        yield return StartCoroutine(DisplayMenu(SaveMenuComponent.Spawn(canvas.gameObject, SaveMenuComponent.SaveMenuMode.Load, () => {
            StartCoroutine(ResumeRoutine());
        })));
    }

    private IEnumerator DisplayMenu(GameObject menuObject) {
        suspended = true;
        MenuComponent menuComponent = menuObject.GetComponent<MenuComponent>();
        menuComponent.Alpha = 0.0f;

        yield return CoUtils.RunParallel(new[] {
            textbox.FadeOutRoutine(this, PauseMenuComponent.FadeoutSeconds),
            paragraphBox.FadeOutRoutine(this, PauseMenuComponent.FadeoutSeconds)
        }, this);

        yield return menuComponent.FadeInRoutine();
    }

    private IEnumerator PlayCurrentScript() {
        playingRoutine = currentScript.PerformActions(this);
        yield return StartCoroutine(playingRoutine);
    }

    private IEnumerator SetHiddenTextModeRoutine(bool hidden) {
        Global.Instance().Input.DisableListener(this);

        if (hidden) {
            yield return paragraphBox.FadeOutRoutine(this, hiddenTextModeFadeoutSeconds);
            yield return textbox.FadeOutRoutine(this, hiddenTextModeFadeoutSeconds);
        } else {
            yield return paragraphBox.FadeInRoutine(this, hiddenTextModeFadeoutSeconds);
            yield return textbox.FadeInRoutine(this, hiddenTextModeFadeoutSeconds);
        }

        hiddenTextMode = hidden;
        Global.Instance().Input.EnableListener(this);
    }
}
