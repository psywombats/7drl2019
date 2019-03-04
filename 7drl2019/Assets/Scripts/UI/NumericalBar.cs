using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class NumericalBar : MonoBehaviour {

    public SliderBar bar;
    public Text max;
    public Text actual;
    [Tooltip("in units per second")]
    public float defaultSpeed = 50.0f;

    private float currentValue;

    public void Populate(float max, float actual) {
        this.max.text = ((int)max).ToString();
        this.actual.text = ((int)actual).ToString();
        bar.Populate(max, actual);
        currentValue = actual;
    }

    public IEnumerator AnimateWithTimeRoutine(float max, float actual, float duration) {
        Tweener tween = DOTween.To(() => currentValue, (float x) => {
            currentValue = x;
            this.actual.text = Mathf.RoundToInt(x).ToString();
        }, actual, duration);
        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(tween),
            bar.AnimateWithTimeRoutine(Mathf.Max(Mathf.Min(actual, max), 0.0f) / max, duration),
        }, this);
    }

    public IEnumerator AnimateWithSpeedRoutine(float max, float actual, float unitsPerSecond = 0.0f) {
        float speed = unitsPerSecond > 0 ? unitsPerSecond : defaultSpeed;
        yield return AnimateWithTimeRoutine(max, actual, Mathf.Abs(currentValue - actual) / speed);
    }
}
