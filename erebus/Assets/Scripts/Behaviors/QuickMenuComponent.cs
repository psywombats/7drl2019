using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(FadingUIComponent))]
public class QuickMenuComponent : MonoBehaviour {

    public Button menuButton;
    public Button saveButton;
    public Button loadButton;
    public Toggle autoToggle;
    public Toggle skipToggle;

    private ScenePlayer player;

    public void Awake() {
        player = FindObjectOfType<ScenePlayer>();

        FormatButtonForCommand(menuButton, InputManager.Command.Menu);
        FormatButtonForCommand(saveButton, InputManager.Command.Save);
        FormatButtonForCommand(loadButton, InputManager.Command.Load);

        autoToggle.onValueChanged.AddListener((bool value) => {
            player.AutoMode = value;
        });
        skipToggle.onValueChanged.AddListener((bool value) => {
            player.SkipMode = value;
        });
    }

    public void Update() {
        autoToggle.isOn = player.AutoMode;
        skipToggle.isOn = player.SkipMode;
        autoToggle.interactable = player.IsAutoAvailable();
        skipToggle.interactable = player.IsSkipAvailable();
    }

    private void FormatButtonForCommand(Button button, InputManager.Command command) {
        button.onClick.AddListener(() => {
            Global.Instance().input.SimulateCommand(command);
        });
    }

    public IEnumerator FadeInRoutine(float durationSeconds) {
        yield return StartCoroutine(GetComponent<FadingUIComponent>().FadeInRoutine(durationSeconds));
    }

    public IEnumerator FadeOutRoutine(float durationSeconds) {
        yield return StartCoroutine(GetComponent<FadingUIComponent>().FadeOutRoutine(durationSeconds));
    }

    public IEnumerator Activate(ScenePlayer player) {
        yield return player.StartCoroutine(GetComponent<FadingUIComponent>().Activate(player));
    }

    public IEnumerator Deactivate(ScenePlayer player) {
        yield return player.StartCoroutine(GetComponent<FadingUIComponent>().Deactivate(player));
    }
}
