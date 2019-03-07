using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(MapEvent3D))]
public class PCVisibility : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    public bool visible { get; private set; }

    public void Start() {
        if (spriteRenderer != null) {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, visible ? 1.0f : 0.0f);
        } else {
           StartCoroutine(GetComponent<CharaEvent>().FadeRoutine(visible));
        }
        
    }

    public void SetVisibleByPC(bool visible) {
        if (this.visible != visible) {
            StartCoroutine(FadeRoutine(visible));
            this.visible = visible;
        }
    }

    public IEnumerator FadeRoutine(bool visible) {
        if (GetComponent<CharaEvent>() != null) {
            return GetComponent<CharaEvent>().FadeRoutine(visible);
        } else {
            Tweener tween = DOTween.To(() => spriteRenderer.color.a, x => {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, x);
            }, visible ? 1.0f : 0.0f, 0.125f);
            return CoUtils.RunTween(tween);
        }
    }
}
