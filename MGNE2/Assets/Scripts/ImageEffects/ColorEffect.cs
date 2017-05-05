using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class ColorEffect : ImageEffectBase {
    public Texture textureRamp;

    public Color color;
    
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        material.SetColor("_Color", color);
        Graphics.Blit(source, destination, material);
    }
}
