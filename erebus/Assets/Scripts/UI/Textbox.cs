using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Textbox : MonoBehaviour {

    [Header("Config")]
    public float boxAnimationSeconds = 0.2f;
    public float backerAnimationSeconds = 0.2f;
    public float combinedAnimDelaySeconds = 0.2f;

    [Space]
    [Header("Hookups")]
    public Text namebox;
    public Text text;
    public RectTransform backer;
    public RectTransform mainBox;

    private float textHeight;
    private float backerAnchor;

    public void Start() {
        backerAnchor = backer.anchorMax.y;
        textHeight = mainBox.sizeDelta.y;

        StartCoroutine(TestRoutine());
    }

    public IEnumerator TestRoutine() {
        while (true) {
            yield return CoUtils.Wait(1.0f);
            yield return EnableRoutine();
            yield return CoUtils.Wait(1.0f);
            yield return DisableRoutine();
        }
    }

    public IEnumerator EnableRoutine() {
        mainBox.sizeDelta.Set(mainBox.sizeDelta.x, 0.0f);
        backer.anchorMax.Set(0.0f, 0.0f);

        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, backerAnchor), backerAnimationSeconds)),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunTween(mainBox.DOSizeDelta(new Vector2(mainBox.sizeDelta.x, textHeight), boxAnimationSeconds))),
        }, this);

    }

    public IEnumerator DisableRoutine() {
        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(mainBox.DOSizeDelta(new Vector2(mainBox.sizeDelta.x, 0.0f), boxAnimationSeconds)),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, 0.0f), backerAnimationSeconds))),
        }, this);
        //gameObject.SetActive(false);
    }
}
