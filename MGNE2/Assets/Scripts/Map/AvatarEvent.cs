using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class AvatarEvent : MonoBehaviour, InputListener {

    private CharaEvent Chara { get { return GetComponent<CharaEvent>(); } }

    public void Start() {
        Global.Instance().input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType == InputManager.Event.Hold) {
            switch (command) {
                case InputManager.Command.Up:
                    Chara.Step(OrthoDir.North);
                    return true;
                case InputManager.Command.Down:
                    Chara.Step(OrthoDir.South);
                    return true;
                case InputManager.Command.Right:
                    Chara.Step(OrthoDir.East);
                    return true;
                case InputManager.Command.Left:
                    Chara.Step(OrthoDir.West);
                    return true;
                default:
                    return false;

            }
        } else {
            return false;
        }
    }
}
