using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PauseMenuComponent : MenuComponent {

    public const float FadeoutSeconds = 0.2f;
    private const string PrefabName = "Prefabs/UI/PauseMenu";
    private const string TitleSceneName = "TitleScene";

    public Button saveButton;
    public Button loadButton;
    public Button resumeButton;
    public Button closeButton;
    public Button titleButton;
    public Button logButton;
    public Button settingsButton;

    public static GameObject Spawn(GameObject parent, Action onFinish) {
        Global.Instance().Memory.RememberScreenshot();
        return Spawn(parent, PrefabName, onFinish);
    }

    public void Awake() {
        saveButton.onClick.AddListener(() => {
            StartCoroutine(SaveRoutine());
        });
        loadButton.onClick.AddListener(() => {
            StartCoroutine(LoadRoutine());
        });
        resumeButton.onClick.AddListener(() => {
            StartCoroutine(ResumeRoutine());
        });
        closeButton.onClick.AddListener(() => {
            StartCoroutine(QuitRoutine());
        });
        titleButton.onClick.AddListener(() => {
            StartCoroutine(TitleRoutine());
        });
        logButton.onClick.AddListener(() => {
            StartCoroutine(LogRoutine());
        });
        settingsButton.onClick.AddListener(() => {
            StartCoroutine(SettingsRoutine());
        });
    }

    protected override void SetInputEnabled(bool enabled) {
        base.SetInputEnabled(enabled);
        saveButton.interactable = enabled;
        loadButton.interactable = enabled;
        resumeButton.interactable = enabled;
        closeButton.interactable = enabled;
        titleButton.interactable = enabled;
        logButton.interactable = enabled;
    }

    private IEnumerator SaveRoutine() {
        yield return StartCoroutine(FadeOutRoutine());
        GameObject saveMenuObject = SaveMenuComponent.Spawn(gameObject.transform.parent.gameObject, SaveMenuComponent.SaveMenuMode.Save, () => {
            StartCoroutine(FadeInRoutine());
        });
        saveMenuObject.GetComponent<SaveMenuComponent>().Alpha = 0.0f;
        yield return StartCoroutine(saveMenuObject.GetComponent<SaveMenuComponent>().FadeInRoutine());
    }

    private IEnumerator LoadRoutine() {
        yield return StartCoroutine(FadeOutRoutine());
        GameObject saveMenuObject = SaveMenuComponent.Spawn(gameObject.transform.parent.gameObject, SaveMenuComponent.SaveMenuMode.Load, () => {
            StartCoroutine(FadeInRoutine());
        });
        saveMenuObject.GetComponent<SaveMenuComponent>().Alpha = 0.0f;
        yield return StartCoroutine(saveMenuObject.GetComponent<SaveMenuComponent>().FadeInRoutine());
    }

    private IEnumerator TitleRoutine() {
        Global.Instance().Input.RemoveListener(this);
        FadeComponent fader = FindObjectOfType<FadeComponent>();
        yield return fader.FadeToBlackRoutine();
        SceneManager.LoadScene(TitleSceneName);
    }

    private IEnumerator QuitRoutine() {
        FadeComponent fader = FindObjectOfType<FadeComponent>();
        yield return fader.FadeToBlackRoutine();
        Application.Quit();
    }

    private IEnumerator LogRoutine() {
        yield return StartCoroutine(FadeOutRoutine());
        GameObject logObject = LogComponent.Spawn(gameObject.transform.parent.gameObject, () => {
            StartCoroutine(FadeInRoutine());
        });
        logObject.GetComponent<LogComponent>().Alpha = 0.0f;
        yield return StartCoroutine(logObject.GetComponent<LogComponent>().FadeInRoutine());
    }

    private IEnumerator SettingsRoutine() {
        yield return StartCoroutine(FadeOutRoutine());
        GameObject settingsObject = SettingsMenuComponent.Spawn(gameObject.transform.parent.gameObject, () => {
            StartCoroutine(FadeInRoutine());
        });
        settingsObject.GetComponent<SettingsMenuComponent>().Alpha = 0.0f;
        yield return StartCoroutine(settingsObject.GetComponent<SettingsMenuComponent>().FadeInRoutine());
    }
}
