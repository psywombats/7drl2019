using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour, InputListener {

    public List<Image> Cursors;

    private int cursorIndex;

    public void Start() {
        cursorIndex = 0;
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
        } else if (cursorIndex >= Cursors.Count) {
            cursorIndex = 0;
        }
        UpdateDisplay();
    }

    private void Confirm() {

    }
}
