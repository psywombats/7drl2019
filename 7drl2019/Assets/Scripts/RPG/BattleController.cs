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
            StartCoroutine(CoUtils.RunAfterDelay(0.1f, () => {
                StartCoroutine(BattleRoutine());
            }));
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
            ui.OnTurn();
            yield return PlayNextHumanActionRoutine();
            map.GetComponent<LineOfSightEffect>().RecalculateVisibilityMap();
            map.GetComponent<LineOfSightEffect>().TransitionFromOldLos(
                1.0f / pcEvent.GetComponent<MapEvent>().CalcTilesPerSecond());
            
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
                yield return unit.OnNewTurnRoutine();
            }
        }
    }

    private IEnumerator PlayNextHumanActionRoutine() {
        while (pcEvent.GetComponent<MapEvent>().tracking) {
            yield return null;
        }
        Result<bool> executeResult = new Result<bool>();
        yield return ui.PlayNextCommandRoutine(executeResult);
        if (!executeResult.canceled) {
            if (!cleared) {
                foreach (BattleUnit unit in new List<BattleUnit>(units)) {
                    if (unit == pc) {
                        continue;
                    } else {
                        if (unit.isRecovering) {
                            unit.isRecovering = false;
                        } else {
                            yield return unit.ai.TakeTurnRoutine();
                        }
                    }
                }

                // 7drl hack
                foreach (BattleUnit unit in new List<BattleUnit>(units)) {
                    if (unit.canActAgain) {
                        unit.canActAgain = false;
                        yield return unit.ai.TakeTurnRoutine();
                    }
                }
            }
        }
        if (cleared) {
            cleared = false;
        }
    }

    public BattleUnit AddUnitFromMap(BattleEvent battler) {
        BattleUnit unit = new BattleUnit(Instantiate(battler.unitSerialized), this);
        battler.unit = unit;
        units.Add(unit);
        battlers[battler.unit] = battler;
        return unit;
    }

    private void AddUnitsFromMap() {
        foreach (BattleEvent battler in map.GetEvents<BattleEvent>()) {
            if (battler.GetComponent<PCEvent>() == null) {
                AddUnitFromMap(battler);
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

    public DirectionCursor SpawnDirectionCursor(Vector2Int location, bool cameraFollows = false) {
        dirCursor.cameraFollows = false;
        dirCursor.Enable();
        dirCursor.GetComponent<MapEvent>().SetLocation(location);
        return dirCursor;
    }

    public void DespawnCursor() {
        cursor.Disable();
    }

    public void DespawnDirCursor() {
        dirCursor.Disable();
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
