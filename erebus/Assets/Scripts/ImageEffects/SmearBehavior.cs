using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class SmearBehavior : MonoBehaviour {

    private Material material;
    private Sprite sprite;

    // in a given column x, will sample from (x, lows[x]) to (x, highs[x])
    private float[] smearLows;
    private float[] smearHighs;
    private int tipRow;

    public void Awake() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        material = renderer.sharedMaterial;
        RedrawSmearMap();
    }

    public void Update() {
        if (GetComponent<SpriteRenderer>().sprite != sprite) {
            RedrawSmearMap();
        }
        AssignCommonShaderVariables();
    }

    private void RedrawSmearMap() {
        sprite = GetComponent<SpriteRenderer>().sprite;
        
        if (sprite == null) {
            return;
        }

        smearLows = new float[sprite.texture.width];
        smearHighs = new float[sprite.texture.width];

        Vector2 pivotPx = sprite.pivot;
        Vector2Int pivot = new Vector2Int((int)pivotPx.x, (int)pivotPx.y);
        tipRow = -1;
        for (int y = 0; y < sprite.texture.height; y += 1) {
            Color[] pixels = sprite.texture.GetPixels(0, y, sprite.texture.width, 1);
            bool foundLow = false;
            bool foundHigh = false;
            for (int x = 0; x < sprite.texture.width; x += 1) {
                Color c = pixels[x];
                if (c.a > 0 && !foundLow) {
                    foundLow = true;
                    smearLows[y] = x;
                } else if (c.a < 1 && foundLow && !foundHigh) {
                    foundHigh = true;
                    smearHighs[y] = x;
                } else if (c.a > 0 && foundHigh) {
                    foundHigh = false;
                    smearLows[y] = x;
                }
            }
            if (foundLow && tipRow == -1) {
                tipRow = y;
            }
            if (IsBlackish(pixels[(int)smearLows[y]]) && smearHighs[y] - smearLows[y] > 1.0f) {
                smearLows[y] += 1;
            }
            if (IsBlackish(pixels[(int)smearHighs[y]]) && smearHighs[y] - smearLows[y] > 1.0f) {
                smearHighs[y] -= 1;
            }
        }
    }

    private void AssignCommonShaderVariables() {
        if (smearLows != null && smearLows.Length > 0) {
            material.SetInt("_PivotX", (int)sprite.pivot.x);
            material.SetInt("_PivotY", (int)sprite.pivot.y);
            material.SetFloatArray("_SmearLows", smearLows);
            material.SetFloatArray("_SmearHighs", smearHighs);
            material.SetInt("_TipRow", tipRow);
        }
    }

    private bool IsBlackish(Color c) {
        return c.r < 0.1f && c.g < 0.1f && c.b < 0.1f;
    }
}
