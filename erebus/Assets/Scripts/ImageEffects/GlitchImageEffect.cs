using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
public class GlitchImageEffect : MonoBehaviour {

    public Material material;
    public bool UseWaveSource;

    private float elapsedSeconds;

    public void Update() {
        AssignCommonShaderVariables();
        elapsedSeconds += Time.deltaTime;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        material.SetTexture("_MainTexture", source);
        AssignCommonShaderVariables();
        Graphics.Blit(source, destination, material);
    }

    private void AssignCommonShaderVariables() {
        material.SetFloat("_Elapsed", elapsedSeconds);
        if (UseWaveSource && Global.Instance().Audio.GetWaveSource() != null) {
            material.SetFloatArray("_Wave", Global.Instance().Audio.GetWaveSource().GetSamples());
            material.SetInt("_WaveSamples", Global.Instance().Audio.GetWaveSource().GetSamples().Length);
        }
    }
}
