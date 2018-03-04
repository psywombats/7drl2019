using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class ColorEffect : MonoBehaviour {

    public Color color;
    
    // A unity version update broke this, reimplement if needed
    //void OnRenderImage(RenderTexture source, RenderTexture destination) {
    //    material.SetColor("_Color", color);
    //    Graphics.Blit(source, destination, material);
    //}

    public void SetColor(Color newColor) {
        color = newColor;
    }

    public IEnumerator FadeRoutine(Color target, float seconds) {
        Color original = color;
        for (float elapsed = 0; elapsed < seconds; elapsed += Time.deltaTime) {
            float t = elapsed / seconds;
            color.r = original.r * (1.0f - t) + target.r * t;
            color.g = original.g * (1.0f - t) + target.g * t;
            color.b = original.b * (1.0f - t) + target.b * t;
            yield return null;
        }
        color = target;
    }
}
