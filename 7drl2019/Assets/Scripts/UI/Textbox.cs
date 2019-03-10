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
    public Text namebox1;
    public Text namebox2;
    public Text textbox;
    public Facebox facebox;
    public Facebox facebox2;
    public RectTransform backer;
    public RectTransform mainBox;
    public GameObject advanceArrow;

    [Space]
    [Header("Autogenerated-ish")]
    public float textHeight;
    public float backerAnchor;

    public bool isDisplaying { get; private set; }

    private bool hurried;
    private bool confirmed;
    private BattleUnit unit1, unit2;

    public void Start() {
        textbox.text = "";
        mainBox.sizeDelta = new Vector2(mainBox.sizeDelta.x, 0.0f);
        backer.anchorMax = new Vector2(0.5f, 0.0f);
        advanceArrow.SetActive(false);
    }

    public void OnValidate() {
        backerAnchor = backer.anchorMax.y;
        textHeight = mainBox.sizeDelta.y;
    }

    public void ConfigureSpeakers(BattleUnit unit1, BattleUnit unit2) {
        this.unit1 = unit1;
        this.unit2 = unit2;
        namebox1.text = unit1.ToString();
        namebox2.text = unit2.ToString();
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
        isDisplaying = true;
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
        yield return SpeakRoutine(speakerName, text, 0);
    }
    public IEnumerator SpeakRoutine(string speakerName, string text, int faceNo) {
        namebox1.text = unit1.ToString();
        namebox2.text = unit2.ToString();
        if (!isDisplaying) {
            yield return EnableRoutine(speakerName);
        } else {
            yield return EraseTextRoutine(textClearSeconds);
            if (namebox1.text != speakerName) {
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    CloseBoxRoutine(boxAnimationSeconds),
                    EraseName1Routine(boxAnimationSeconds),
                }, this);
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    OpenBoxRoutine(boxAnimationSeconds),
                    ShowName2Routine(boxAnimationSeconds),
                }, this);
            } else if (namebox2.text != speakerName) {
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    CloseBoxRoutine(boxAnimationSeconds),
                    EraseName2Routine(boxAnimationSeconds),
                }, this);
                yield return CoUtils.RunParallel(new IEnumerator[] {
                    OpenBoxRoutine(boxAnimationSeconds),
                    ShowName1Routine(boxAnimationSeconds),
                }, this);
            }
        }

        if (facebox) {
            if (speakerName == "Pri") {
                facebox.SetFaceNumber(faceNo);
            } else {
                facebox2.SetFaceNumber(faceNo);
            }
        }

        yield return TypeRoutine(text);
    }

    public IEnumerator DisableRoutine() {
        facebox.SetFaceNumber(0);
        isDisplaying = false;
        yield return CoUtils.RunParallel(new IEnumerator[] {
            EraseName1Routine(boxAnimationSeconds / 2.0f),
            EraseName2Routine(boxAnimationSeconds / 2.0f),
            EraseTextRoutine(boxAnimationSeconds / 2.0f),
            CloseBoxRoutine(boxAnimationSeconds),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, 0.0f), backerAnimationSeconds))),
        }, this);
        Global.Instance().Input.RemoveListener(this);
    }

    public IEnumerator EnableRoutine(string firstSpeaker, bool useNameboxes = true) {
        isDisplaying = true;
        Global.Instance().Input.PushListener(this);

        namebox1.transform.parent.GetComponent<CanvasGroup>().alpha = 0.0f;
        namebox2.transform.parent.GetComponent<CanvasGroup>().alpha = 0.0f;

        if (useNameboxes) {
            yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, backerAnchor), backerAnimationSeconds)),
            CoUtils.Delay(combinedAnimDelaySeconds,
                CoUtils.RunParallel(new IEnumerator[] {
                    firstSpeaker == unit1.ToString()
                        ? ShowName1Routine(boxAnimationSeconds)
                        : ShowName2Routine(boxAnimationSeconds),
                    OpenBoxRoutine(boxAnimationSeconds),
                }, this)),
            }, this);
        } else {
            yield return CoUtils.RunParallel(new IEnumerator[] {
                CoUtils.RunTween(backer.DOAnchorMax(new Vector2(0.5f, backerAnchor), backerAnimationSeconds)),
                CoUtils.Delay(combinedAnimDelaySeconds, OpenBoxRoutine(boxAnimationSeconds)),
            }, this);
        }
    }

    private IEnumerator ShowName1Routine(float seconds) {
        yield return CoUtils.RunTween(namebox1.transform.parent.GetComponent<CanvasGroup>().DOFade(1.0f, seconds));
    }

    private IEnumerator ShowName2Routine(float seconds) {
        yield return CoUtils.RunTween(namebox2.transform.parent.GetComponent<CanvasGroup>().DOFade(1.0f, seconds));
    }

    private IEnumerator EraseName1Routine(float seconds) {
        yield return CoUtils.RunTween(namebox1.transform.parent.GetComponent<CanvasGroup>().DOFade(0.0f, seconds));
    }

    private IEnumerator EraseName2Routine(float seconds) {
        yield return CoUtils.RunTween(namebox2.transform.parent.GetComponent<CanvasGroup>().DOFade(0.0f, seconds));
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
