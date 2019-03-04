using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharaEvent))]
public class PCEvent : MonoBehaviour {

    private int pauseCount;
    public bool InputPaused {
        get {
            return pauseCount > 0;
        }
    }

    private MapEvent parent { get { return GetComponent<MapEvent>(); } }

    public void Start() {
        Global.Instance().Maps.pc = this;
        pauseCount = 0;
    }

    public void PauseInput() {
        pauseCount += 1;
    }

    public void UnpauseInput() {
        pauseCount -= 1;
    }

    private void Interact() {
        Vector2Int target = GetComponent<MapEvent>().location + GetComponent<CharaEvent>().facing.XY();
        List<MapEvent> targetEvents = GetComponent<MapEvent>().map.GetEventsAt(target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.switchEnabled && !tryTarget.IsPassableBy(parent)) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }

        target = GetComponent<MapEvent>().location;
        targetEvents = GetComponent<MapEvent>().map.GetEventsAt(target);
        foreach (MapEvent tryTarget in targetEvents) {
            if (tryTarget.switchEnabled && tryTarget.IsPassableBy(parent)) {
                tryTarget.GetComponent<Dispatch>().Signal(MapEvent.EventInteract, this);
                return;
            }
        }
    }

    private void ShowMenu() {
        // oh shiii
    }
}
