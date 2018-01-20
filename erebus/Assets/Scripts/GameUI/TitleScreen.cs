using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour, InputListener {

    private const float FadeSeconds = 0.6f;

    public List<Image> Cursors;

    private ColorEffect fader;
    private ColorEffect Fader {
        get {
            if (fader == null) {
                fader = FindObjectOfType<ColorEffect>();
            }
            return fader;
        }
    }

    private int cursorIndex;

    public void Start() {
        cursorIndex = (Global.Instance().Memory.AnyMemoriesExist()) ? 1 : 0;
        UpdateDisplay();
        Global.Instance().Input.PushListener(this);
    }

    public void UpdateDisplay() {
        for (int i = 0; i < Cursors.Count; i += 1) {
            Image cursor = Cursors[i];
            cursor.enabled = (i == cursorIndex);
        }
    }
    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Down) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Down:
            case InputManager.Command.Right:
                MoveCursor(1);
                return true;
            case InputManager.Command.Left:
            case InputManager.Command.Up:
                MoveCursor(-1);
                return true;
            case InputManager.Command.Confirm:
                Confirm();
                return true;
            default:
                return true;
        }
    }

    private void MoveCursor(int delta) {
        cursorIndex += delta;
        if (cursorIndex < 0) {
            cursorIndex = Cursors.Count;
        } else if (cursorIndex > Cursors.Count) {
            cursorIndex = 0;
        }
        UpdateDisplay();
    }

    private void Confirm() {
        switch (cursorIndex) {
            case 0:
                NewGame();
                break;
            case 1:
                LoadGame();
                break;
        }
    }

    private void NewGame() {
        Global.Instance().Input.RemoveListener(this);
        StartCoroutine(CoUtils.RunWithCallback(TransitionOutRoutine(), this, () => {
            SceneManager.LoadScene("Scenes/Main", LoadSceneMode.Single);
            Global.Instance().Memory.StartCoroutine(CoUtils.RunAfterDelay(0.0f, () => {
                Global.Instance().Maps.Camera.GetComponent<ColorEffect>().SetColor(Color.black);
                IEnumerator routine = Global.Instance().Maps.Camera.GetComponent<ColorEffect>().FadeRoutine(Color.white, FadeSeconds);
                Global.Instance().Memory.StartCoroutine(routine);
            }));
        }));
    }

    private void LoadGame() {
        Global.Instance().Input.RemoveListener(this);
        StartCoroutine(CoUtils.RunWithCallback(TransitionOutRoutine(), this, () => {
            SceneManager.LoadScene("Scenes/Main", LoadSceneMode.Single);
            Global.Instance().Memory.StartCoroutine(CoUtils.RunAfterDelay(0.0f, () => {
                Global.Instance().Maps.Camera.GetComponent<ColorEffect>().SetColor(Color.black);
                Global.Instance().Memory.LoadMemory(Global.Instance().Memory.GetMemoryForSlot(0));
                IEnumerator routine = Global.Instance().Maps.Camera.GetComponent<ColorEffect>().FadeRoutine(Color.white, FadeSeconds);
                Global.Instance().Memory.StartCoroutine(routine);
            }));
        }));
    }

    private IEnumerator TransitionOutRoutine() {
        yield return StartCoroutine(Fader.FadeRoutine(Color.black, FadeSeconds));
    }
}
