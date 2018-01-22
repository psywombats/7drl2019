using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DurationEffectBehavior : MonoBehaviour {

    private float elapsedSeconds;

    public void Update() {
        AssignCommonShaderVariables();
        elapsedSeconds += Time.deltaTime;
    }

    public Material GetMaterial() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        return renderer.sharedMaterial;
    }

    private void AssignCommonShaderVariables() {
        GetMaterial().SetFloat("_Elapsed", elapsedSeconds);
    }
}
