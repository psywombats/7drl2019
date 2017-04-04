using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapEvent))]
[RequireComponent(typeof(CharaAnimator))]
public class AvatarEvent : MonoBehaviour, InputListener {

    public float PixelsPerSecond = 32.0f;

    private Vector2 targetLocation;

    public void Start() {
        Global.Instance().input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType == InputManager.Event.Down) {
            switch (command) {
                case InputManager.Command.Up:
                    Step(OrthoDir.North);
                    return true;
                case InputManager.Command.Right:
                    Step(OrthoDir.East);
                    return true;
                case InputManager.Command.Down:
                    Step(OrthoDir.South);
                    return true;
                case InputManager.Command.Left:
                    Step(OrthoDir.West);
                    return true;
                default:
                    return false;

            }
        } else {
            return false;
        }
    }

    public void Update() {
        if (targetLocation != null) {
            //Vector2 direction = targetLocation - GetComponent<MapEvent>().PixelPosition;
        }
    }

    private void Step(OrthoDir dir) {
        
    }
}
