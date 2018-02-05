using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(FadingUIComponent))]
public class SpeakerDisplayComponent : MonoBehaviour {

    public CharaData chara;
    public Image portraitImage;
    public Text nametag;

    public void SetChara(CharaData chara) {
        this.chara = chara;
        if (chara != null) {
            portraitImage.sprite = chara.portrait;
            nametag.text = chara.name;
        } else {
            chara = null;
        }
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
