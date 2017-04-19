using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class AvatarEvent : MonoBehaviour, InputListener {

    public bool InputPaused { get; set; }

    public void Start() {
        Global.Instance().Input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (GetComponent<CharaEvent>().Tracking || InputPaused) {
            return true;
        }
        if (eventType == InputManager.Event.Hold) {
            switch (command) {
                case InputManager.Command.Up:
                    TryStep(OrthoDir.North);
                    return true;
                case InputManager.Command.Down:
                    TryStep(OrthoDir.South);
                    return true;
                case InputManager.Command.Right:
                    TryStep(OrthoDir.East);
                    return true;
                case InputManager.Command.Left:
                    TryStep(OrthoDir.West);
                    return true;
                default:
                    return false;

            }
        } else {
            return false;
        }
    }

    public bool TryStep(OrthoDir dir) {
        IntVector2 target = GetComponent<MapEvent>().Position + dir.XY();
        GetComponent<CharaEvent>().Facing = dir;
        MapEvent targetEvent = GetComponent<MapEvent>().Parent.GetEventAt(GetComponent<MapEvent>().Layer, target);

        if (GetComponent<CharaEvent>().IsPassableAt(target)) {
            GetComponent<CharaEvent>().Step(dir, () => {
                if (targetEvent != null) {
                    targetEvent.OnCollide(this);
                }
            });
        } else {
            if (targetEvent != null) {
                targetEvent.OnCollide(this);
            }
        }

        return true;
    }
}
