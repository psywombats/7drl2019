using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class SliderBar : MonoBehaviour {

    public Mask backer;
    public Image bar;
    public int inset = 16;
    public float ratio = 0.5f;
    [Tooltip("in units per second")]
    public float defaultSpeed = 1.0f;

    public void OnValidate() {
        UpdateScale();
    }

    public void Populate(float max, float actual) {
        ratio = actual / max;
        UpdateScale();
    }

    private void UpdateScale() {
        float maxOffset = bar.rectTransform.rect.width - inset;
        bar.rectTransform.localPosition = new Vector3(
            -1.0f * (1.0f - ratio) * maxOffset,
            bar.rectTransform.localPosition.y,
            bar.rectTransform.localPosition.z);
    }

    public IEnumerator AnimateWithTimeRoutine(float target, float duration) {
        Tweener tween = DOTween.To(() => ratio, (float x) => {
            ratio = x;
            UpdateScale();
        }, Mathf.Max(Mathf.Min(1.0f, target), 0.0f), duration);
        yield return CoUtils.RunTween(tween);
    }

    public IEnumerator AnimateWithSpeedRoutine(float target, float unitsPerSecond = 0.0f) {
        float speed = unitsPerSecond > 0 ? unitsPerSecond : defaultSpeed;
        yield return AnimateWithTimeRoutine(target, Mathf.Abs(ratio - target) / speed);
    }
}
