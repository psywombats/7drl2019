using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(FadingUIComponent))]
public class SpeakerComponent : MonoBehaviour {

    public CharaData chara;
    public SpeakerDisplayComponent front;
    public SpeakerDisplayComponent back;
    public float transitionTime = 0.1f;

    private ScenePlayer player;

    public void Awake() {
        player = FindObjectOfType<ScenePlayer>();
    }

    public void SetChara(CharaData chara) {
        if (chara == this.chara && chara != null) {
            return;
        }

        this.chara = chara;

        back.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
        if (chara != null) {
            front.GetComponent<FadingUIComponent>().SetAlpha(1.0f);
        } else {
            front.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
        }
        front.SetChara(chara);
    }

    public void TransitionToChara(CharaData chara) {
        if (chara == this.chara) {
            return;
        }

        this.chara = chara;
        
        if (chara != null) {
            back.SetChara(front.chara);
            back.GetComponent<FadingUIComponent>().SetAlpha(front.GetComponent<FadingUIComponent>().GetAlpha());
            front.SetChara(chara);
            front.GetComponent<FadingUIComponent>().SetAlpha(0.0f);

            if (player != null && player.ShouldUseFastMode()) {
                front.GetComponent<FadingUIComponent>().SetAlpha(1.0f);
                back.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
            } else {
                StartCoroutine(back.GetComponent<FadingUIComponent>().FadeOutRoutine(transitionTime));
                StartCoroutine(front.GetComponent<FadingUIComponent>().FadeInRoutine(transitionTime));
            }
        } else {
            if (player != null && player.ShouldUseFastMode()) {
                front.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
                back.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
            } else {
                StartCoroutine(front.GetComponent<FadingUIComponent>().FadeOutRoutine(transitionTime));
                StartCoroutine(back.GetComponent<FadingUIComponent>().FadeOutRoutine(transitionTime));
            }

        }
    }

    public bool HasChara() {
        return chara != null;
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
