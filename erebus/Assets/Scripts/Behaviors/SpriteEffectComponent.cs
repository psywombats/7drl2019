using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteEffectComponent : MonoBehaviour {

    private const string WhiteoutBackgroundTag = "whiteout";

    public GameObject background;
    public GameObject midground;
    public GameObject foreground;

    public TextboxComponent letterboxText;

    public GameObject whiteoutBackground;
    public GameObject whiteoutMidground;
    public GameObject whiteoutForeground;

    public FadingUIComponent letterboxTop;
    public FadingUIComponent letterboxBottom;

    private GameObject activeBackground;
    private GameObject activeMidground;
    private GameObject activeForeground;

    private ScenePlayer player;

    public void Awake() {
        this.player = FindObjectOfType<ScenePlayer>();
        HideLetterboxes();
    }

    public IEnumerator StartWhiteoutRoutine(float duration) {
        List<SpriteEffectControlComponent> controls = new List<SpriteEffectControlComponent>();

        activeBackground = Instantiate(whiteoutBackground);
        activeMidground = Instantiate(whiteoutMidground);
        activeForeground = Instantiate(whiteoutForeground);
        activeBackground.transform.parent = background.transform;
        activeMidground.transform.parent = midground.transform;
        activeForeground.transform.parent = foreground.transform;
        controls.Add(activeBackground.GetComponent<SpriteEffectControlComponent>());
        controls.Add(activeMidground.GetComponent<SpriteEffectControlComponent>());
        controls.Add(activeForeground.GetComponent<SpriteEffectControlComponent>());

        // 
        player.background.SetBackground(WhiteoutBackgroundTag);
        foreach (SpriteEffectControlComponent control in controls) {
            control.Alpha = 0.0f;
        }
        while (duration > 0 && controls[0].Alpha < 1.0f) {
            foreach (SpriteEffectControlComponent control in controls) {
                control.Alpha += Time.deltaTime / duration;
            }
            yield return null;
        }
        foreach (SpriteEffectControlComponent control in controls) {
            control.Alpha = 1.0f;
        }
    }

    public IEnumerator StopWhiteoutRoutine(float duration) {
        List<SpriteEffectControlComponent> controls = new List<SpriteEffectControlComponent>();
        controls.Add(activeBackground.GetComponent<SpriteEffectControlComponent>());
        controls.Add(activeMidground.GetComponent<SpriteEffectControlComponent>());
        controls.Add(activeForeground.GetComponent<SpriteEffectControlComponent>());

        while (duration > 0 && controls[0].Alpha > 0.0f) {
            foreach (SpriteEffectControlComponent control in controls) {
                control.Alpha -= Time.deltaTime / duration;
            }
            yield return null;
        }
        foreach (SpriteEffectControlComponent control in controls) {
            Destroy(control.gameObject);
        }
    }

    public IEnumerator FadeLetterboxesIn() {
        yield return StartCoroutine(CoUtils.RunParallel(new[] {
            letterboxTop.Activate(),
            letterboxBottom.Activate()
        }, this));
    }

    public void HideLetterboxes() {
        letterboxTop.gameObject.SetActive(false);
        letterboxBottom.gameObject.SetActive(false);
    }
}
