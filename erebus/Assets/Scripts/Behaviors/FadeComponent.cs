using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeComponent : MonoBehaviour {

    public Image image = null;
    public bool autoFadeIn = false;
    public float fadeTime = 0.8f;

    private BGMPlayer bgm;

    private float Alpha {
        get { return image.color.a; }
        set { image.color = new Color(image.color.r, image.color.g, image.color.b, value); }
    }

    public void Awake() {
        bgm = FindObjectOfType<BGMPlayer>();
        if (autoFadeIn) {
            Alpha = 1.0f;
        }
    }

    public void Start() {
        if (autoFadeIn) {
            StartCoroutine(RemoveTintRoutine());
            autoFadeIn = false;
        }
    }
    
    public IEnumerator FadeToBlackRoutine(bool allowFastMode = false, bool fadeBGM = true) {
        gameObject.transform.SetAsLastSibling();
        image.CrossFadeAlpha(1.0f, fadeTime, false);
        if (bgm != null && fadeBGM) {
            StartCoroutine(bgm.FadeOutRoutine(fadeTime));
        }
        yield return new WaitForSeconds(fadeTime);
    }

    public IEnumerator RemoveTintRoutine(bool allowFastMode = false) {
        gameObject.transform.SetAsLastSibling();
        image.CrossFadeAlpha(0.0f, fadeTime, false);
        yield return new WaitForSeconds(fadeTime);
    }
}
