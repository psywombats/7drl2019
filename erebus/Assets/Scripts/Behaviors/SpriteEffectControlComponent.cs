using UnityEngine;
using System.Collections;

public class SpriteEffectControlComponent : MonoBehaviour {

    public ParticleSystem particle;
    public SpriteRenderer sprite;

    public float Alpha {
        get {
            //if (sprite != null) {
            //    return sprite.color.a;
            //} else {
            //    return particle.startColor.a;
            //}
            return 1.0f;
        }
        set {
            //if (sprite != null) {
            //    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, value);
            //}
            //if (particle != null) {
            //    Color oldColor = particle.GetComponent<Renderer>().material.color;
            //    Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, value);
            //    particle.GetComponent<Renderer>().material.color = newColor;
            //}
        }
    }
}
