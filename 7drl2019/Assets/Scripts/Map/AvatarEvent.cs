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

    private MapEvent parent { get { return GetComponent<MapEvent>(); } }

    public void Start() {
        Global.Instance().Maps.avatar = this;
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
        memory.position = GetComponent<MapEvent>().location;
        memory.facing = GetComponent<CharaEvent>().facing;
    }

    public void PauseInput() {
        pauseCount += 1;
    }

    public void UnpauseInput() {
        pauseCount -= 1;
    }

    private void Interact() {
        Vector2Int target = GetComponent<MapEvent>().location + GetComponent<CharaEvent>().facing.XY2D();
        List<MapEvent> targetEvents = GetComponent<MapEvent>().parent.GetEventsAt(target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.switchEnabled && !tryTarget.IsPassableBy(parent)) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }

        target = GetComponent<MapEvent>().location;
        targetEvents = GetComponent<MapEvent>().parent.GetEventsAt(target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.switchEnabled && tryTarget.IsPassableBy(parent)) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }
    }

    private bool TryStep(OrthoDir dir) {
        Vector2Int vectors = GetComponent<MapEvent>().location;
        Vector2Int vsd = dir.XY3D();
        Vector2Int target = vectors + vsd;
        GetComponent<CharaEvent>().facing = dir;
        List<MapEvent> targetEvents = GetComponent<MapEvent>().parent.GetEventsAt(target);

        if (GetComponent<BattleEvent>() && !GetComponent<BattleEvent>().CanCrossTileGradient(parent.location, target)) {
            return false;
        }

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = parent.CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            if (!parent.CanPassAt(target)) {
                passable = false;
            }
        }

        if (passable) {
            StartCoroutine(CoUtils.RunWithCallback(GetComponent<MapEvent>().StepRoutine(dir), () => {
                foreach (MapEvent targetEvent in toCollide) {
                    if (targetEvent.switchEnabled) {
                        targetEvent.GetComponent<Dispatch>().Signal(MapEvent.EventCollide, this);
                    }
                }
            }));
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                if (targetEvent.switchEnabled && !targetEvent.IsPassableBy(parent)) {
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
