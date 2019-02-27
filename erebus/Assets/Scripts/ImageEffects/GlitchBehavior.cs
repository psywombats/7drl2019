using UnityEngine;
using UnityEngine.UI;

public class GlitchBehavior : MonoBehaviour {
    public bool useWaveSource;

    private Material material;
    private float elapsedSeconds;

    public void Awake() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) {
            material = renderer.sharedMaterial;
        } else {
            material = GetComponent<Image>().material;
        }
    }

    public void Update() {
        AssignCommonShaderVariables();
        elapsedSeconds += Time.deltaTime;
    }

    private void AssignCommonShaderVariables() {
        material.SetFloat("_Elapsed", elapsedSeconds);
        if (useWaveSource && Global.Instance().Audio.GetWaveSource() != null) {
            material.SetFloatArray("_Wave", Global.Instance().Audio.GetWaveSource().GetSamples());
            material.SetInt("_WaveSamples", Global.Instance().Audio.GetWaveSource().GetSampleCount());
        }
    }
}
