using UnityEngine;
using System.Collections;
using System;

public class TransitionComponent : MonoBehaviour {

    public Shader shader;

    private FadeData currentFade;
    private Material material;
    private float elapsedSeconds;
    private float transitionDuration;
    private bool reverse;
    private bool active;

    public void Awake() {
        material = new Material(shader);
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

    public Material GetMaterial() {
        return material;
    }

    public bool IsTransitioning() {
        return active;
    }

    public void Hurry() {
        if (this.elapsedSeconds > 0.0f) {
            this.elapsedSeconds = transitionDuration;
        }
        AssignCommonShaderVariables();
        active = false;
    }

    public IEnumerator TransitionRoutine(TransitionData transition, Action intermediate = null) {
        yield return StartCoroutine(FadeRoutine(transition.GetFadeOut()));
        if (intermediate != null) {
            intermediate();
        }
        yield return StartCoroutine(FadeRoutine(transition.GetFadeIn(), true));
    }

    public IEnumerator FadeRoutine(FadeData fade, bool invert = false, float timeMult = 1.0f) {
        this.currentFade = fade;
        this.reverse = invert;
        elapsedSeconds = 0.0f;
        transitionDuration = fade.delay * timeMult;
        active = true;
        AssignCommonShaderVariables();

        ScenePlayer player = FindObjectOfType<ScenePlayer>();
        while (elapsedSeconds < transitionDuration) {
            if (player.ShouldUseFastMode()) {
                break;
            }
            yield return null;
        }
        AssignCommonShaderVariables();
    }

    private void AssignCommonShaderVariables() {
        if (currentFade != null) {
            float elapsed = elapsedSeconds / transitionDuration;
            material.SetTexture("_MaskTexture", currentFade.transitionMask);
            material.SetFloat("_Elapsed", reverse ? (1.0f-elapsed) : elapsed);
            material.SetFloat("_SoftFudge", currentFade.softEdgePercent);
            material.SetInt("_Invert", currentFade.invert ? 1 : 0);
            material.SetInt("_FlipX", currentFade.flipHorizontal ? 1 : 0);
            material.SetInt("_FlipY", currentFade.flipVertical ? 1 : 0);
        }
    }
}
