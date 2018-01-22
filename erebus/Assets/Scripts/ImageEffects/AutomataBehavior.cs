using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class AutomataBehavior : MonoBehaviour {

    private float elapsedSeconds;
    private RenderTexture accumTexture;

    public void OnDestroy() {
        DestroyImmediate(accumTexture);
    }

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

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Texture source = renderer.sprite.texture;

        // Create the accumulation texture
        if (source != null && (accumTexture == null || accumTexture.width != source.width || accumTexture.height != source.height)) {
            DestroyImmediate(accumTexture);
            accumTexture = new RenderTexture(source.width, source.height, 0);
            accumTexture.hideFlags = HideFlags.HideAndDontSave;
            Graphics.Blit(source, accumTexture);
        }

        if (accumTexture != null) {
            GetMaterial().SetTexture("_BufferTex", source);
            GetMaterial().mainTexture = accumTexture;
        }
    }
}
