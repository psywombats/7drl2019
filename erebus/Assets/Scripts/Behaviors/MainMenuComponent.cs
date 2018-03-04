using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuComponent : MenuComponent {
    
    public Button StartButton;
    public Button LoadButton;
    public Button QuitButton;
    public Button SettingsButton;
    public Button ContinueButton;

    public void Awake() {
        StartButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            Global.Instance().UIEngine.StartCoroutine(StartRoutine());
        });
        LoadButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(LoadRoutine());
        });
        QuitButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(QuitRoutine());
        });
        ContinueButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(ContinueRoutine());
        });
        SettingsButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(SettingsRoutine());
        });
    }

    public override void Start() {
        base.Start();
        StartCoroutine(InitializeRoutine());
    }

    protected override void SetInputEnabled(bool enabled) {
        base.SetInputEnabled(enabled);
        StartButton.interactable = enabled;
        LoadButton.interactable = enabled;
        QuitButton.interactable = enabled;
        SettingsButton.interactable = enabled;
        ContinueButton.interactable = enabled;
    }

    private IEnumerator StartRoutine() {
        AsyncOperation op = SceneManager.LoadSceneAsync("Main");
        op.allowSceneActivation = false;
        yield return CoUtils.RunParallel(new[] {
            Global.Instance().UIEngine.GlobalFadeRoutine(true),
            Global.Instance().Audio.FadeOutRoutine(Global.Instance().UIEngine.Tint.FadeSeconds),
        }, this);
        op.allowSceneActivation = true;
        while (!op.isDone) {
            yield return null;
        }
        LuaScript script = Global.Instance().Lua.CreateScriptFromFile("intro");
        yield return script.RunRoutine();
    }

    private IEnumerator LoadRoutine() {
        yield return new WaitForSeconds(0.1f);
        GameObject loadMenuObject = SaveMenuComponent.Spawn(gameObject.transform.parent.gameObject, SaveMenuComponent.SaveMenuMode.Load, () => {
            SetInputEnabled(true);
        });
        loadMenuObject.GetComponent<SaveMenuComponent>().Alpha = 0.0f;
        yield return StartCoroutine(loadMenuObject.GetComponent<SaveMenuComponent>().FadeInRoutine());
    }

    private IEnumerator QuitRoutine() {
        yield return Global.Instance().UIEngine.Tint.Activate();
        Application.Quit();
    }

    private IEnumerator ContinueRoutine() {
        Global.Instance().Input.RemoveListener(this);
        Global.Instance().Memory.LoadFromLastSaveSlot();
        yield return null;
    }

    private IEnumerator SettingsRoutine() {
        yield return new WaitForSeconds(0.1f);
        GameObject settingsObject = SettingsMenuComponent.Spawn(gameObject.transform.parent.gameObject, () => {
            SetInputEnabled(true);
        });
        settingsObject.GetComponent<SettingsMenuComponent>().Alpha = 0.0f;
        yield return StartCoroutine(settingsObject.GetComponent<SettingsMenuComponent>().FadeInRoutine());
    }

    private IEnumerator InitializeRoutine() {
        Global.Instance().Audio.PlayBGM("noise");
        yield return Global.Instance().UIEngine.Tint.Activate();
        SetInputEnabled(true);
    }
}
