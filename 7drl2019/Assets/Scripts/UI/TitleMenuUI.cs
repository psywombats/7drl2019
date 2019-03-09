using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleMenuUI : MonoBehaviour, InputListener {

    public List<TitleOptionBox> options;
    public FadeImageEffect fader;
    public CanvasGroup help;

    private int selection;
    private bool halting, helping;

    public void Start() {
        foreach (TitleOptionBox box in options) {
            box.selectionBacker.enabled = false;
        }
        MoveSelection(0);

        Global.Instance().Input.PushListener(this);
        GetComponent<CanvasGroup>().alpha = 0.0f;
        help.alpha = 0.0f;
        StartCoroutine(CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f)));
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up || halting) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Up:
                MoveSelection(-1);
                break;
            case InputManager.Command.Down:
                MoveSelection(1);
                break;
            case InputManager.Command.Confirm:
                if (helping) {
                    HideHelp();
                } else {
                    switch (selection) {
                        case 0:
                            StartGame();
                            break;
                        case 1:
                            ShowHelp();
                            break;
                        case 2:
                            Quit();
                            break;
                    }
                }
                break;
        }
        return true;
    }

    private void MoveSelection(int delta) {
        options[selection].selectionBacker.enabled = false;
        selection += delta;
        if (selection == 0) {
            selection = 0;
        } else if (selection == options.Count) {
            selection = options.Count - 1;
        }
        options[selection].selectionBacker.enabled = true;
    }

    private void StartGame() {
        halting = true;
        StartCoroutine(CoUtils.RunWithCallback(CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 1.0f)),
            fader.FadeRoutine(fader.startFade, false),
        }, this), () => {
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
        }));
    }

    private void ShowHelp() {
        halting = true;
        StartCoroutine(CoUtils.RunWithCallback(CoUtils.RunSequence(new IEnumerator[] {
            CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 0.5f)),
            CoUtils.RunTween(help.DOFade(1.0f, 0.5f)),
        }), () => {
            halting = false;
            helping = true;
        }));
    }

    private void HideHelp() {
        halting = true;
        StartCoroutine(CoUtils.RunWithCallback(CoUtils.RunSequence(new IEnumerator[] {
            CoUtils.RunTween(help.DOFade(0.0f, 0.5f)),
            CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(1.0f, 0.5f)),
        }), () => {
            halting = false;
            helping = false;
        }));
    }

    private void Quit() {
        halting = true;
        StartCoroutine(CoUtils.RunWithCallback(CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 1.0f)),
            fader.FadeRoutine(fader.startFade, !fader.startFade.invert),
        }, this), () => {
            Application.Quit();
        }));
    }
}
