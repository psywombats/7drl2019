using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class FadeImageEffect : MonoBehaviour {

    public Shader shader;
    public FadeData startFade;

    private FadeData currentFade;
    private Material material;
    private float elapsedSeconds;
    private float transitionDuration;
    private bool reverse;
    private bool active;

    public void Awake() {
        if (GetComponent<Camera>() == null) {
            material = GetComponent<Image>().material;
        } else {
            material = new Material(shader);
        }
    }

    public void Start() {
        if (startFade != null) {
            StartCoroutine(FadeRoutine(startFade, true));
        }
    }

    public void OnDestroy() {
        Destroy(material);
    }

    public void Update() {
        if (active) {
            if (GetComponent<Camera>() == null) {
                AssignCommonShaderVariables();
            }
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds > transitionDuration) {
                elapsedSeconds = transitionDuration;
                active = false;
            }
        }
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (GetComponent<Camera>() != null) {
            material.SetTexture("_MainTexture", source);
            AssignCommonShaderVariables();
            Graphics.Blit(source, destination, material);
        }
    }

    public IEnumerator TransitionRoutine(TransitionData transition, Action intermediate = null) {
        yield return StartCoroutine(FadeRoutine(transition.GetFadeOut()));
        intermediate?.Invoke();
        yield return StartCoroutine(FadeRoutine(transition.GetFadeIn(), true));
    }

    public IEnumerator FadeRoutine(FadeData fade, bool invert = false, float timeMult = 1.0f) {
        currentFade = fade;
        reverse = invert;
        elapsedSeconds = 0.0f;
        transitionDuration = fade.delay * timeMult;
        active = true;
        AssignCommonShaderVariables();

        while (elapsedSeconds < transitionDuration) {
            yield return null;
        }
        AssignCommonShaderVariables();
    }

    private void AssignCommonShaderVariables() {
        if (currentFade != null) {
            float elapsed = elapsedSeconds / transitionDuration;
            material.SetTexture("_MaskTexture", currentFade.transitionMask);
            material.SetFloat("_Elapsed", reverse ? (1.0f - elapsed) : elapsed);
            material.SetFloat("_SoftFudge", currentFade.softEdgePercent);
            material.SetInt("_Invert", currentFade.invert ? 1 : 0);
            material.SetInt("_FlipX", currentFade.flipHorizontal ? 1 : 0);
            material.SetInt("_FlipY", currentFade.flipVertical ? 1 : 0);
        }
    }
}
