using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(TransitionComponent))]
public class FadingUIComponent : MonoBehaviour {

    private static readonly string FadeInKeyDefault = "fade";
    private static readonly string FadeOutKeyDefault = "fade";

    public float FadeSeconds = 0.5f;
    public float FastModeFadeSeconds = 0.15f;
    public string fadeInKey;
    public string fadeOutKey;

    private float fadeDurationSeconds;
    private float targetAlpha;

    private float Alpha {
        get { return gameObject.GetComponent<CanvasGroup>().alpha; }
        set { gameObject.GetComponent<CanvasGroup>().alpha = value; }
    }

    public void Awake() {
        targetAlpha = Alpha;
    }

    public void Start() {
        if (GetComponent<Image>() != null) {
            GetComponent<Image>().material = GetComponent<TransitionComponent>().GetMaterial();
        }
    }

    public void Update() {
        if (Alpha < targetAlpha) {
            Alpha += Time.deltaTime / fadeDurationSeconds;
            if (Alpha > targetAlpha) Alpha = targetAlpha;
        } else if (Alpha > targetAlpha) {
            Alpha -= Time.deltaTime / fadeDurationSeconds;
            if (Alpha < targetAlpha) Alpha = targetAlpha;
        }
    }

    public void SetAlpha(float alpha) {
        Alpha = alpha;
        targetAlpha = alpha;
    }

    public float GetAlpha() {
        return Alpha;
    }

    public IEnumerator FadeInRoutine(float durationSeconds) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }
        this.fadeDurationSeconds = durationSeconds;
        this.targetAlpha = 1.0f;
        while (Alpha != targetAlpha) {
            yield return null;
        }
    }

    public IEnumerator FadeOutRoutine(float durationSeconds) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }
        this.fadeDurationSeconds = durationSeconds;
        this.targetAlpha = 0.0f;
        while (Alpha != targetAlpha) {
            yield return null;
        }
    }

    public IEnumerator Activate(ScenePlayer player = null) {
        gameObject.SetActive(true);
        SetAlpha(0.0f);
        FadeData fadeIn = new FadeData(Global.Instance().Database.Fades.GetData(fadeInKey.Length == 0 ? FadeInKeyDefault : fadeInKey));
        TransitionComponent transition = GetComponent<TransitionComponent>();
        if (Alpha < 1.0f) {
            fadeIn.delay = GetFadeSeconds(player);
            StartCoroutine(transition.FadeRoutine(fadeIn));
            yield return null;
            SetAlpha(1.0f);
            while (transition.IsTransitioning()) {
                if (player != null && player.WasHurried()) {
                    transition.Hurry();
                }
                yield return null;
            }
        }
    }

    public IEnumerator Deactivate(ScenePlayer player = null) {
        FadeData fadeOut = new FadeData(Global.Instance().Database.Fades.GetData(fadeInKey.Length == 0 ? FadeOutKeyDefault : fadeOutKey));
        TransitionComponent transition = GetComponent<TransitionComponent>();
        if (Alpha > 0.0f) {
            fadeOut.delay = GetFadeSeconds(player);
            StartCoroutine(transition.FadeRoutine(fadeOut, true));
            yield return null;
            while (transition.IsTransitioning()) {
                if (player != null && player.WasHurried()) {
                    transition.Hurry();
                }
                yield return null;
            }
        }
        gameObject.SetActive(false);
    }

    private float GetFadeSeconds(ScenePlayer player) {
        if (player != null && player.ShouldUseFastMode()) {
            return FastModeFadeSeconds;
        } else {
            return FadeSeconds;
        }
    }
}
