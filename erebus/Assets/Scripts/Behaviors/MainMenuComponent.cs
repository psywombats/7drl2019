using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuComponent : MenuComponent {
    
    public Button startButton;
    public Button loadButton;
    public Button quitButton;
    public Button settingsButton;
    public Button continueButton;

    private FadeComponent fade;

    public void Awake() {
        fade = FindObjectOfType<FadeComponent>();

        startButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(StartRoutine());
        });
        loadButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(LoadRoutine());
        });
        quitButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(QuitRoutine());
        });
        continueButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(ContinueRoutine());
        });
        settingsButton.onClick.AddListener(() => {
            SetInputEnabled(false);
            StartCoroutine(SettingsRoutine());
        });

        StartCoroutine(CoUtils.RunAfterDelay(FindObjectOfType<FadeComponent>().fadeTime, () => {
            SetInputEnabled(true);
        }));
    }

    protected override void SetInputEnabled(bool enabled) {
        base.SetInputEnabled(enabled);
        startButton.interactable = enabled;
        loadButton.interactable = enabled;
        quitButton.interactable = enabled;
    }

    private IEnumerator StartRoutine() {
        yield return fade.FadeToBlackRoutine();
        //ScenePlayer.LoadScreen();
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
        yield return fade.FadeToBlackRoutine();
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
}
