using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GlitchBehavior : MonoBehaviour {
    public Shader shader;
    
    private Material material;
    public bool UseWaveSource;
    private float elapsedSeconds;

    public void Awake() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        material = renderer.sharedMaterial;
    }

    public void Update() {
        AssignCommonShaderVariables();
        elapsedSeconds += Time.deltaTime;
    }

    public Material GetMaterial() {
        return material;
    }

    private void AssignCommonShaderVariables() {
        material.SetFloat("_Elapsed", elapsedSeconds);
        if (UseWaveSource && Global.Instance().Audio.GetWaveSource() != null) {
            material.SetFloatArray("_Wave", Global.Instance().Audio.GetWaveSource().GetSamples());
            material.SetInt("_WaveSamples", Global.Instance().Audio.GetWaveSource().GetSamples().Length);
        }
    }
}
