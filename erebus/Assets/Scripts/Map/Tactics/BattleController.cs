using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/**
 * Responsible for user input and rendering during a battle. Control flow is actually handled by
 * the Battle class.
 */
[RequireComponent(typeof(Map))]
public class BattleController : MonoBehaviour {

    private const string ListenerId = "BattleControllerListenerId";

    // properties required upon initializion
    public Battle battle;

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }
    public Cursor cursor { get; private set; }
    public DirectionCursor dirCursor { get; private set; }

    // internal state
    private Dictionary<BattleUnit, BattleEvent> dolls;

    // === INITIALIZATION ==========================================================================

    public BattleController() {
        dolls = new Dictionary<BattleUnit, BattleEvent>();
    }

    public void Start() {
        AddUnitsFromMap();

        cursor = Cursor.GetInstance();
        cursor.gameObject.transform.parent = GetComponent<Map>().objectLayer.transform;
        cursor.gameObject.SetActive(false);

        dirCursor = DirectionCursor.GetInstance();
        dirCursor.gameObject.transform.parent = GetComponent<Map>().objectLayer.transform;
        dirCursor.gameObject.SetActive(false);
    }

    private void AddUnitsFromMap() {
        foreach (BattleEvent battler in map.GetEvents<BattleEvent>()) {
            BattleUnit unit = new BattleUnit(battler.unitData, battle, battler.GetComponent<MapEvent>().position);
            battler.Setup(this, unit);
        }
    }

    // === GETTERS AND BOOKKEEPING =================================================================

    public BattleEvent GetDollForUnit(BattleUnit unit) {
        return dolls[unit];
    }

    public BattleUnit GetUnitAt(Vector2Int position) {
        foreach (MapEvent mapEvent in map.GetEventsAt(position)) {
            if (mapEvent.GetComponent<BattleEvent>() != null) {
                return mapEvent.GetComponent<BattleEvent>().unit;
            }
        }
        return null;
    }

    // === STATE MACHINE ===========================================================================

    public IEnumerator TurnBeginAnimationRoutine(Alignment align) {
        yield break;
    }

    public IEnumerator TurnEndAnimationRoutine(Alignment align) {
        List<IEnumerator> routinesToRun = new List<IEnumerator>();
        foreach (BattleUnit unit in battle.UnitsByAlignment(align)) {
            routinesToRun.Add(unit.doll.PostTurnRoutine());
        }
        yield return CoUtils.RunParallel(routinesToRun.ToArray(), this);
    }

    // it's a discrete step in the human's turn, they should be able to undo up to this point
    public IEnumerator PlayNextHumanActionRoutine() {
        MoveCursorToDefaultUnit();

        BattleUnit actingUnit = null;
        while (actingUnit == null) {
            Result<BattleUnit> unitResult = new Result<BattleUnit>();
            yield return SelectUnitRoutine(unitResult, (BattleUnit unit) => {
                return unit.align == Alignment.Hero;
            }, false);
            actingUnit = unitResult.value;
            Vector2Int originalLocation = actingUnit.location;

            Result<Vector2Int> moveResult = new Result<Vector2Int>();
            yield return SelectMoveLocationRoutine(moveResult, actingUnit);
            if (moveResult.canceled) {
                continue;
            }
            actingUnit.location = moveResult.value;
            yield return actingUnit.doll.GetComponent<CharaEvent>().PathToRoutine(moveResult.value);

            // TODO: remove this nonsense
            Result<BattleUnit> targetedResult = new Result<BattleUnit>();
            yield return dirCursor.SelectAdjacentUnitRoutine(targetedResult, actingUnit, (BattleUnit unit) => {
                return unit.align == Alignment.Enemy;
            });
            if (targetedResult.canceled) {
                // TODO: reset where they came from
                actingUnit.location = originalLocation;
                break;
            }
            BattleUnit targetUnit = targetedResult.value;
            targetUnit.doll.GetComponent<CharaEvent>().FaceToward(actingUnit.location);

            yield return Global.Instance().Maps.activeDuelMap.EnterMapRoutine(actingUnit.doll, targetUnit.doll);

            actingUnit.MarkActionTaken();
            yield return Global.Instance().Maps.activeDuelMap.ExitMapRoutine();
            yield return actingUnit.doll.PostActionRoutine();
        }
    }
    
    // cancelable, awaits user selecting a unit that matches the rule
    private IEnumerator SelectUnitRoutine(Result<BattleUnit> result, 
            Func<BattleUnit, bool> rule, 
            bool allowCancel=true) {
        cursor.gameObject.SetActive(true);
        while (!result.finished) {
            Result<Vector2Int> locResult = new Result<Vector2Int>();
            yield return cursor.AwaitSelectionRoutine(locResult);
            if (locResult.canceled && allowCancel) {
                result.Cancel();
                break;
            }
            BattleUnit unit = GetUnitAt(locResult.value);
            if (unit != null && rule(unit)) {
                result.value = unit;
            }
        }
        cursor.gameObject.SetActive(false);
    }

    // selects a move location for the selected unit, might be canceled
    private IEnumerator SelectMoveLocationRoutine(Result<Vector2Int> result, BattleUnit unit, bool canCancel=true) {
        cursor.gameObject.SetActive(true);
        cursor.GetComponent<MapEvent>().SetLocation(unit.location);

        SelectionGrid grid = SpawnSelectionGrid();
        int range = (int)unit.Get(StatTag.MOVE);
        Func<Vector2Int, bool> rule = (Vector2Int loc) => {
            if (loc == unit.location) {
                return false;
            }
            return map.FindPath(unit.doll.GetComponent<CharaEvent>(), loc, range) != null;
        };
        grid.ConfigureNewGrid(new Vector2Int(range, range), map.terrain, rule);

        while (!result.finished) {
            Result<Vector2Int> locResult = new Result<Vector2Int>();
            yield return cursor.AwaitSelectionRoutine(locResult);
            if (locResult.canceled && canCancel) {
                result.Cancel();
            } else {
                if (rule(locResult.value)) {
                    result.value = locResult.value;
                }
            }
        }

        cursor.gameObject.SetActive(false);
        Destroy(grid.gameObject);
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

    public void TargetCameraToLocation(Vector2Int loc) {
        TacticsCam.Instance().SetTargetLocation(loc);
    }

    public SelectionGrid SpawnSelectionGrid() {
        SelectionGrid grid = SelectionGrid.GetInstance();
        grid.gameObject.transform.parent = GetComponent<Map>().objectLayer.transform;
        return grid;
    }

    private void MoveCursorToDefaultUnit() {
        BattleUnit defaultHero = battle.GetFaction(Alignment.Hero).NextMoveableUnit();
        cursor.GetComponent<MapEvent>().SetLocation(defaultHero.location);
    }
}
