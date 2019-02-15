﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Textbox : MonoBehaviour, InputListener {

    private static readonly string SystemSpeaker = "SYSTEM";

    [Header("Config")]
    public float charsPerSecond = 120f;
    public float boxAnimationSeconds = 0.2f;
    public float backerAnimationSeconds = 0.2f;
    public float combinedAnimDelaySeconds = 0.2f;
    public float textClearSeconds = 0.1f;

    [Space]
    [Header("Hookups")]
    public Text namebox;
    public Text textbox;
    public RectTransform backer;
    public RectTransform mainBox;
    public GameObject advanceArrow;

    private float textHeight;
    private float backerAnchor;

    private bool displaying;
    private bool hurried;
    private bool confirmed;

    public void Start() {
        backerAnchor = backer.anchorMax.y;
        textHeight = mainBox.sizeDelta.y;

        StartCoroutine(TestRoutine());
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        switch (eventType) {
            case InputManager.Event.Down:
                if (command == InputManager.Command.Confirm) {
                    hurried = true;
                }
                break;
            case InputManager.Event.Up:
                if (command == InputManager.Command.Confirm) {
                    confirmed = true;
                }
                break;
        }
        return true;
    }

    public IEnumerator TestRoutine() {
        displaying = true;
        while (true) {
            yield return DisableRoutine();
            yield return CoUtils.Wait(1.0f);
            yield return SpeakRoutine("Diaghilev", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do " +
                "eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud " +
                "exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.");
            yield return SpeakRoutine("Diaghilev", "Etc.");
            yield return SpeakRoutine("Homasa", "Hello I am someone completely different");
        }
    }

    public IEnumerator SpeakRoutine(string text) {
        yield return SpeakRoutine(SystemSpeaker, text);
    }

    public IEnumerator SpeakRoutine(string speakerName, string text) {
        if (!displaying) {
            namebox.enabled = speakerName != SystemSpeaker;
            namebox.text = speakerName;
            yield return EnableRoutine();
        } else {
            yield return EraseTextRoutine(textClearSeconds);
            if (namebox.text != speakerName) {
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    CloseBoxRoutine(boxAnimationSeconds),
                    EraseNameRoutine(boxAnimationSeconds),
                }, this);
                namebox.enabled = speakerName != SystemSpeaker;
                namebox.text = speakerName;
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    OpenBoxRoutine(boxAnimationSeconds),
                    ShowNameRoutine(boxAnimationSeconds),
                }, this);
            }
        }

        yield return TypeRoutine(text);
    }

    public IEnumerator DisableRoutine() {
        displaying = false;
        yield return CoUtils.RunParallel(new IEnumerator[] {
            EraseNameRoutine(boxAnimationSeconds / 2.0f),
            EraseTextRoutine(boxAnimationSeconds / 2.0f),
            CloseBoxRoutine(boxAnimationSeconds),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, 0.0f), backerAnimationSeconds))),
        }, this);
    }

    private IEnumerator EnableRoutine() {
        displaying = true;
        textbox.text = "";
        mainBox.sizeDelta.Set(mainBox.sizeDelta.x, 0.0f);
        backer.anchorMax.Set(0.0f, 0.0f);
        advanceArrow.SetActive(false);

        Global.Instance().Input.PushListener(this);

        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, backerAnchor), backerAnimationSeconds)),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunParallel(new IEnumerator[] {
                    ShowNameRoutine(boxAnimationSeconds),
                    OpenBoxRoutine(boxAnimationSeconds),
                }, this)),
        }, this);
    }

    private IEnumerator ShowNameRoutine(float seconds) {
        yield return CoUtils.RunTween(namebox.GetComponent<CanvasGroup>().DOFade(1.0f, seconds));
    }

    private IEnumerator EraseNameRoutine(float seconds) {
        yield return CoUtils.RunTween(namebox.GetComponent<CanvasGroup>().DOFade(0.0f, seconds));
    }

    private IEnumerator EraseTextRoutine(float seconds) {
        yield return CoUtils.RunTween(textbox.GetComponent<CanvasGroup>().DOFade(0.0f, seconds));
    }

    private IEnumerator OpenBoxRoutine(float seconds) {
        yield return CoUtils.RunTween(mainBox.DOSizeDelta(new Vector2(mainBox.sizeDelta.x, textHeight), seconds));
    }

    private IEnumerator CloseBoxRoutine(float seconds) {
        yield return CoUtils.RunTween(mainBox.DOSizeDelta(new Vector2(mainBox.sizeDelta.x, 0.0f), seconds));
    }

    private IEnumerator TypeRoutine(string text) {
        hurried = false;
        float elapsed = 0.0f;
        float total = text.Length / charsPerSecond;
        textbox.GetComponent<CanvasGroup>().alpha = 1.0f;
        while (elapsed <= total) {
            elapsed += Time.deltaTime;
            int charsToShow = Mathf.FloorToInt(elapsed * charsPerSecond);
            int cutoff = charsToShow > text.Length ? text.Length : charsToShow;
            textbox.text = text.Substring(0, cutoff);
            textbox.text += "<color=#00000000>";
            textbox.text += text.Substring(cutoff);
            textbox.text += "</color>";
            yield return null;

            if (hurried) {
                break;
            }
        }
        textbox.text = text;

        confirmed = false;
        advanceArrow.SetActive(true);
        while (!confirmed) {
            yield return null;
        }
        advanceArrow.SetActive(false);
    }
}
