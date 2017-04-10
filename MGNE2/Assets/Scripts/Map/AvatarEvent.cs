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
        if (Chara.tracking) {
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
        IntVector2 target = Chara.Event.Position + dir.XY();

        if (Chara.PassableAt(target)) {
            Chara.Step(dir);
        } else {
            Chara.Facing = dir;
        }

        return true;
    }
}
