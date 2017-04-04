using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapEvent))]
[RequireComponent(typeof(CharaAnimator))]
public class AvatarEvent : MonoBehaviour, InputListener {

    private MapEvent Event { get { return GetComponent<MapEvent>(); } }

    public void Start() {
        Global.Instance().input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType == InputManager.Event.Hold) {
            switch (command) {
                case InputManager.Command.Up:
                    Event.Step(OrthoDir.North);
                    return true;
                case InputManager.Command.Right:
                    Event.Step(OrthoDir.East);
                    return true;
                case InputManager.Command.Down:
                    Event.Step(OrthoDir.South);
                    return true;
                case InputManager.Command.Left:
                    Event.Step(OrthoDir.West);
                    return true;
                default:
                    return false;

            }
        } else {
            return false;
        }
    }
}
