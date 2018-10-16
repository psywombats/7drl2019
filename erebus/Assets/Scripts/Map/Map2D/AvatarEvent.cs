using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class AvatarEvent : MonoBehaviour, InputListener, MemoryPopulater {

    private int pauseCount;
    public bool InputPaused {
        get {
            return pauseCount > 0;
        }
    }

    public void Start() {
        Global.Instance().Input.PushListener(this);
        pauseCount = 0;
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (GetComponent<MapEvent>().tracking || InputPaused) {
            return true;
        }
        switch (eventType) {
            case InputManager.Event.Hold:
                switch (command) {
                    case InputManager.Command.Up:
                        TryStep(OrthoDir.North);
                        return false;
                    case InputManager.Command.Down:
                        TryStep(OrthoDir.South);
                        return false;
                    case InputManager.Command.Right:
                        TryStep(OrthoDir.East);
                        return false;
                    case InputManager.Command.Left:
                        TryStep(OrthoDir.West);
                        return false;
                    default:
                        return false;
                }
            case InputManager.Event.Up:
                switch (command) {
                    case InputManager.Command.Confirm:
                        Interact();
                        return false;
                    case InputManager.Command.Cancel:
                        ShowMenu();
                        return false;
                    case InputManager.Command.Debug:
                        Global.Instance().Memory.SaveToSlot(0);
                        return false;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    public void PopulateFromMemory(Memory memory) {
        GetComponent<MapEvent>().SetLocation(memory.position);
        GetComponent<CharaEvent>().facing = memory.facing;
    }

    public void PopulateMemory(Memory memory) {
        memory.position = GetComponent<MapEvent>().Position;
        memory.facing = GetComponent<CharaEvent>().facing;
    }

    public void PauseInput() {
        pauseCount += 1;
    }

    public void UnpauseInput() {
        pauseCount -= 1;
    }

    private void Interact() {
        IntVector2 target = GetComponent<MapEvent>().Position + GetComponent<CharaEvent>().facing.XY();
        List<MapEvent> targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.SwitchEnabled && !tryTarget.IsPassableBy(GetComponent<CharaEvent>())) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }

        target = GetComponent<MapEvent>().Position;
        targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.SwitchEnabled && tryTarget.IsPassableBy(GetComponent<CharaEvent>())) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }
    }

    private bool TryStep(OrthoDir dir) {
        IntVector2 target = GetComponent<MapEvent>().Position + dir.XY();
        GetComponent<CharaEvent>().facing = dir;
        List<MapEvent> targetEvents = GetComponent<MapEvent>().Parent.GetEventsAt(GetComponent<MapEvent>().Layer, target);

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = GetComponent<CharaEvent>().CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            if (!GetComponent<CharaEvent>().CanPassAt(target)) {
                passable = false;
            }
        }

        if (passable) {
            StartCoroutine(CoUtils.RunWithCallback(GetComponent<MapEvent>().StepRoutine(dir), this, () => {
                foreach (MapEvent targetEvent in toCollide) {
                    if (targetEvent.SwitchEnabled) {
                        targetEvent.GetComponent<Dispatch>().Signal(MapEvent.EventCollide, this);
                    }
                }
            }));
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                if (targetEvent.SwitchEnabled) {
                    targetEvent.GetComponent<Dispatch>().Signal(MapEvent.EventCollide, this);
                }
            }
        }
        
        return true;
    }

    private void ShowMenu() {
        // oh shiii
    }
}
