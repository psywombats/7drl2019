using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Map))]
public class BattleController : MonoBehaviour {

    public bool immediateMode = true;

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }
    public Cursor cursor { get; private set; }
    public DirectionCursor dirCursor { get; private set; }
    public RogueUI ui { get; private set; }

    public BattleEvent pcEvent;
    public BattleUnit pc { get; private set; }

    // internal state
    public bool started { get; private set; }
    public List<BattleUnit> units;
    private Dictionary<BattleUnit, BattleEvent> battlers;
    private bool cleared;

    public void Log(string message, bool outOfTurn = false) { ui.narrator.Log(message, outOfTurn); }

    // === INITIALIZATION ==========================================================================

    public void Start() {
        Global.Instance().Maps.pc = pcEvent.GetComponent<PCEvent>();

        pc = new BattleUnit(Instantiate(pcEvent.unitSerialized), this);
        pcEvent.unit = pc;
        units = new List<BattleUnit>() { pcEvent.unit };
        battlers = new Dictionary<BattleUnit, BattleEvent>();
        battlers[pcEvent.unit] = pcEvent;

        cursor = Cursor.GetInstance();
        cursor.gameObject.transform.SetParent(GetComponent<Map>().objectLayer.transform);
        cursor.gameObject.SetActive(false);

        dirCursor = DirectionCursor.GetInstance();
        dirCursor.gameObject.transform.SetParent(GetComponent<Map>().objectLayer.transform);
        dirCursor.gameObject.SetActive(false);

        ui = FindObjectOfType<RogueUI>();
        ui.Populate();

        AddUnitsFromMap();

        if (immediateMode) {
            StartCoroutine(BattleRoutine());
        }
    }

    // we're about to teleport off of this map
    public void Clear() {
        units.Clear();
        units.Add(pc);

        battlers.Clear();
        battlers[pcEvent.unit] = pcEvent;
        cleared = true;
    }

    // === GETTERS AND BOOKKEEPING =================================================================

    public BattleEvent GetEventForBattler(BattleUnit unit) {
        return battlers[unit];
    }

    public BattleUnit GetUnitAt(Vector2Int position) {
        foreach (MapEvent mapEvent in map.GetEventsAt(position)) {
            if (mapEvent.GetComponent<BattleEvent>() != null) {
                return mapEvent.GetComponent<BattleEvent>().unit;
            }
        }
        return null;
    }

    public void RemoveUnit(BattleUnit unit) {
        units.Remove(unit);
        // don't get rid of the doll, it's in the process of animating off
    }

    // === STATE MACHINE ===========================================================================

    public IEnumerator BattleRoutine() {
        started = true;
        GetComponent<LineOfSightEffect>().RecalculateVisibilityMap();
        while (true) {
            yield return PlayNextHumanActionRoutine();
            map.GetComponent<LineOfSightEffect>().RecalculateVisibilityMap();
            map.GetComponent<LineOfSightEffect>().TransitionFromOldLos(
                1.0f / pcEvent.GetComponent<MapEvent>().CalcTilesPerSecond());

            List<IEnumerator> toExecute = new List<IEnumerator>();
            foreach (BattleEvent battler in battlers.Values.ToArray()) {
                if (battler.unit.IsDead()) {
                    if (units.Contains(battler.unit)) {
                        units.Remove(battler.unit);
                    }
                    battlers.Remove(battler.unit);
                    map.RemoveEvent(battler.GetComponent<MapEvent>());
                }
            }
            foreach (BattleUnit unit in units) {
                IEnumerator result = unit.OnNewTurnAction();
                if (result != null) {
                    toExecute.Add(result);
                }
            }
            if (toExecute.Count > 0) {
                yield return CoUtils.RunParallel(toExecute.ToArray(), this);
            }
        }
    }

    private IEnumerator PlayNextHumanActionRoutine() {
        Result<IEnumerator> executeResult = new Result<IEnumerator>();
        yield return ui.PlayNextCommand(executeResult);
        if (!executeResult.canceled) {
            List<IEnumerator> animations = new List<IEnumerator>();
            animations.Add(executeResult.value);
            if (!cleared) {
                // grab everyone else's stuff
                foreach (BattleUnit unit in new List<BattleUnit>(units)) {
                    if (unit == pc) {
                        continue;
                    } else {
                        IEnumerator aiAction = unit.ai.TakeTurnAction();
                        if (aiAction != null) {
                            animations.Add(aiAction);
                        }
                    }
                }
                animations.Add(ui.OnTurnAction());
            }
            yield return CoUtils.RunParallel(animations.ToArray(), this);
        }
        if (cleared) {
            AddUnitsFromMap();
            cleared = false;
        }
    }

    private void AddUnitsFromMap() {
        foreach (BattleEvent battler in map.GetEvents<BattleEvent>()) {
            if (battler.GetComponent<PCEvent>() == null) {
                BattleUnit unit = new BattleUnit(Instantiate(battler.unitSerialized), this);
                unit.ai = new AIController(unit);
                battler.unit = unit;
                units.Add(unit);
                battlers[battler.unit] = battler;
            }
        }
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

    public Cursor SpawnCursor(Vector2Int location, bool cameraFollows = false) {
        cursor.cameraFollows = false;
        cursor.Enable();
        cursor.GetComponent<MapEvent>().SetLocation(location);
        return cursor;
    }

    public void DespawnCursor() {
        cursor.Disable();
    }

    public void TargetCameraToLocation(Vector2Int loc) {
        Global.Instance().Maps.camera.SetTargetLocation(loc, map.terrain.HeightAt(loc));
    }

    public SelectionGrid SpawnSelectionGrid(bool cameraFollows = false) {
        SelectionGrid grid = SelectionGrid.GetInstance();
        grid.cameraFollows = cameraFollows;
        grid.gameObject.transform.SetParent(GetComponent<Map>().transform);
        return grid;
    }

    public void MoveCursorToHero() {
        cursor.GetComponent<MapEvent>().SetLocation(pc.location);
    }
}
