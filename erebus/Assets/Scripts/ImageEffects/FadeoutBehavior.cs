using UnityEngine;
using System.Collections;

/**
 * Can be attached to anything with a MeshRenderer or a SpriteRenderer but assumes that whatever
 * it's on has a material with a GlitchDiffuseShader. Allows the transparency to move up and down.
 * Used to fade out the irrelevant objects as part of "duel mode" etc.
 */
public class FadeoutBehavior : MonoBehaviour {

    public float alpha = 1.0f;

    private bool tracking;
    private float alphaTarget;

    private Material FindMaterial() {
        if (GetComponent<SpriteRenderer>() != null) {
            if (Application.isPlaying) {
                return GetComponent<SpriteRenderer>().material;
            } else {
                return GetComponent<SpriteRenderer>().sharedMaterial;
            }
        } else if (GetComponent<MeshRenderer>() != null) {
            if (Application.isPlaying) {
                return GetComponent<MeshRenderer>().material;
            } else {
                return GetComponent<MeshRenderer>().sharedMaterial;
            }
        } else {
            Debug.Assert(false);
            return null;
        }
    }

    public void Update() {
        FindMaterial().SetFloat("_Alpha", alpha);
    }

    public void OnValidate() {
        FindMaterial().SetFloat("_Alpha", alpha);
    }

    public IEnumerator FadeRoutine(float target, float duration) {
        float original = alpha;
        float elapsed = 0;
        while (alpha != target) {
            elapsed += Time.deltaTime;
            alpha = Mathf.Lerp(original, target, elapsed / duration);
            yield return null;
        }
    }
}
