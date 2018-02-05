using UnityEngine;

[RequireComponent(typeof(TransitionComponent))]
[RequireComponent(typeof(SpriteRenderer))]
public class FadingSpriteComponent : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private TransitionComponent transition;

    public void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transition = GetComponent<TransitionComponent>();
    }

    public void Start() {
        spriteRenderer.material = transition.GetMaterial();
    }
}
