using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class AvatarEvent : MonoBehaviour {

    private int pauseCount;
    public bool InputPaused {
        get {
            return pauseCount > 0;
        }
    }

    private MapEvent parent { get { return GetComponent<MapEvent>(); } }

    public void Start() {
        Global.Instance().Maps.avatar = this;
        pauseCount = 0;
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

    // result is true if the action consumed a turn
    public IEnumerator TryStepRoutine(EightDir dir, Result<bool> result) {
        Vector2Int vectors = GetComponent<MapEvent>().location;
        Vector2Int vsd = dir.XY();
        Vector2Int target = vectors + vsd;
        GetComponent<CharaEvent>().facing = OrthoDirExtensions.FromEight(dir, GetComponent<CharaEvent>().facing);
        List<MapEvent> targetEvents = GetComponent<MapEvent>().parent.GetEventsAt(target);

        if (!GetComponent<BattleEvent>().CanCrossTileGradient(parent.location, target)) {
            result.value = false;
            yield break;
        }
        
        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = parent.CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            if (!parent.CanPassAt(target)) {
                passable = false;
            }
        }

        // TODO: 7DRL: attack!!
        if (passable) {
            yield return GetComponent<MapEvent>().StepRoutine(dir);
            foreach (MapEvent targetEvent in toCollide) {
                if (targetEvent.switchEnabled) {
                    targetEvent.GetComponent<Dispatch>().Signal(MapEvent.EventCollide, this);
                }
            }
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                if (targetEvent.switchEnabled && !targetEvent.IsPassableBy(parent)) {
                    targetEvent.GetComponent<Dispatch>().Signal(MapEvent.EventCollide, this);
                }
            }
        }
        result.value = true;
    }

    private void ShowMenu() {
        // oh shiii
    }
}
