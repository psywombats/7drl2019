using UnityEngine;
using System.Collections;

/**
 * Can be attached to anything with a MeshRenderer or a SpriteRenderer but assumes that whatever
 * it's on has a material with a GlitchDiffuseShader. Allows the transparency to move up and down.
 * Used to fade out the irrelevant objects as part of "duel mode" etc.
 */
public class FadeoutBehavior : MonoBehaviour {

    public float alpha = 1.0f;

    private Material FindMaterial() {
        if (GetComponent<SpriteRenderer>() != null) {
            return GetComponent<SpriteRenderer>().material;
        } else if (GetComponent<MeshRenderer>() != null) {
            return GetComponent<MeshRenderer>().material;
        } else {
            Debug.Assert(false);
            return null;
        }
    }

    public void Update() {
        FindMaterial().SetFloat("_Alpha", alpha);
    }
}
