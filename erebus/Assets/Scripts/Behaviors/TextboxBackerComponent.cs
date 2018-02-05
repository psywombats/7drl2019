using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(FadingUIComponent))]
[RequireComponent(typeof(TransitionComponent))]
public class TextboxBackerComponent : MonoBehaviour {

    public FadingUIComponent FadingUI {
        get { return GetComponent<FadingUIComponent>(); }
    }

    public TransitionComponent Transition {
        get { return GetComponent<TransitionComponent>(); }
    }

    public float Alpha {
        get { return FadingUI.GetAlpha(); }
        set { FadingUI.SetAlpha(value); }
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
