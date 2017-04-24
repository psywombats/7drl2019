using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class AvatarEvent : MonoBehaviour, InputListener {

    public bool InputPaused { get; set; }

    public void Start() {
        Global.Instance().Input.PushListener(this);
        Global.Instance().Lua.RegisterAvatar(this);
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
                case InputManager.Command.Confirm:
                    Interact();
                    return true;
                default:
                    return false;

            }
        } else {
            return false;
        }
    }

    public void Interact() {
        IntVector2 target = GetComponent<MapEvent>().Position + GetComponent<CharaEvent>().Facing.XY();
        List<MapEvent> targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (!tryTarget.IsPassableBy(GetComponent<CharaEvent>())) {
                tryTarget.OnInteract(this);
                return;
            }
        }

        target = GetComponent<MapEvent>().Position;
        targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.IsPassableBy(GetComponent<CharaEvent>())) {
                tryTarget.OnInteract(this);
                return;
            }
        }
    }

    public bool TryStep(OrthoDir dir) {
        IntVector2 target = GetComponent<MapEvent>().Position + dir.XY();
        GetComponent<CharaEvent>().Facing = dir;
        List<MapEvent> targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = true;
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            if (!GetComponent<CharaEvent>().CanPassAt(target)) {
                passable = false;
            }
        }

        if (passable) {
            StartCoroutine(CoUtils.RunWithCallback(GetComponent<CharaEvent>().StepRoutine(dir), this, () => {
                foreach (MapEvent targetEvent in toCollide) {
                    targetEvent.OnCollide(this);
                }
            }));
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                targetEvent.OnCollide(this);
            }
        }
        
        return true;
    }
}
