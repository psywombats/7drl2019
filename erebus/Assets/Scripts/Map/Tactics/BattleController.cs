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

    // internal state
    private Dictionary<BattleUnit, BattleEvent> dolls;
    private Cursor cursor;
    private DirectionCursor dirCursor;

    // === INITIALIZATION ==========================================================================

    public BattleController() {
        dolls = new Dictionary<BattleUnit, BattleEvent>();
    }

    public void Start() {
        cursor = Cursor.GetInstance();
        cursor.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        cursor.gameObject.SetActive(false);

        dirCursor = DirectionCursor.GetInstance();
        dirCursor.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        dirCursor.gameObject.SetActive(false);
    }

    // this should take a battle memory at some point
    public void Setup(string battleKey) {
        this.battle = Resources.Load<Battle>("Database/Battles/" + battleKey);
        Debug.Assert(this.battle != null, "Unknown battle key " + battleKey);
    }

    // === GETTERS AND BOOKKEEPING =================================================================

    public BattleEvent GetDollForUnit(BattleUnit unit) {
        return dolls[unit];
    }

    public void AddUnitFromTiledEvent(BattleEvent doll, string unitKey) {
        IntVector2 position = doll.GetComponent<MapEvent3D>().Position;
        BattleUnit newUnit = battle.AddUnitFromKey(unitKey, position);
        doll.Setup(this, newUnit);
        dolls[newUnit] = doll;
    }

    public BattleUnit GetUnitAt(IntVector2 position) {
        foreach (MapEvent mapEvent in map.GetEventsAt(map.LowestObjectLayer(), position)) {
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
            actingUnit = unitResult.Value;
            IntVector2 originalLocation = actingUnit.location;

            Result<IntVector2> moveResult = new Result<IntVector2>();
            yield return SelectMoveLocationRoutine(moveResult, actingUnit);
            if (moveResult.canceled) {
                continue;
            }
            actingUnit.location = moveResult.Value;
            yield return actingUnit.doll.GetComponent<CharaEvent>().PathToRoutine(moveResult.Value);

            // TODO: remove this nonsense
            Result<BattleUnit> targetedResult = new Result<BattleUnit>();
            yield return SelectAdjacentUnitRoutine(targetedResult, actingUnit, (BattleUnit unit) => {
                return unit.align == Alignment.Enemy;
            });
            if (targetedResult.canceled) {
                // TODO: reset where they came from
                actingUnit.location = originalLocation;
                break;
            }
            BattleUnit targetUnit = targetedResult.Value;
            targetUnit.doll.GetComponent<CharaEvent>().FaceToward(actingUnit.location);

            yield return Global.Instance().Maps.ActiveDuelMap.EnterMapRoutine(actingUnit.doll, targetUnit.doll);

            actingUnit.MarkActionTaken();
            yield return Global.Instance().Maps.ActiveDuelMap.ExitMapRoutine();
            yield return actingUnit.doll.PostActionRoutine();
        }
    }
    
    // cancelable, awaits user selecting a unit that matches the rule
    private IEnumerator SelectUnitRoutine(Result<BattleUnit> result, 
            Func<BattleUnit, bool> rule, 
            bool allowCancel=true) {
        cursor.gameObject.SetActive(true);
        while (!result.finished) {
            Result<IntVector2> locResult = new Result<IntVector2>();
            yield return cursor.AwaitSelectionRoutine(locResult);
            if (locResult.canceled && allowCancel) {
                result.Cancel();
                break;
            }
            BattleUnit unit = GetUnitAt(locResult.Value);
            if (unit != null && rule(unit)) {
                result.Value = unit;
            }
        }
        cursor.gameObject.SetActive(false);
    }

    // selects a square to be targeted by the acting unit, might be canceled
    private IEnumerator SelectTargetDirRoutine(Result<OrthoDir> result,
            BattleUnit actingUnit,
            List<OrthoDir> allowedDirs,
            bool canCancel = true) { 

        dirCursor.gameObject.SetActive(true);
        dirCursor.currentDir = allowedDirs[0];
        cursor.DisableReticules();

        SelectionGrid grid = SpawnSelectionGrid();
        grid.ConfigureNewGrid(new IntVector2(3, 3), (IntVector2 loc) => {
            return (loc.x + loc.y) % 2 == 1;
        });
        grid.GetComponent<MapEvent>().Position = actingUnit.location - new IntVector2(1, 1);
        grid.GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();
        
        while (!result.finished) {
            Result<OrthoDir> dirResult = new Result<OrthoDir>();
            yield return dirCursor.AwaitSelectionRoutine(actingUnit.doll, dirResult);
            if (dirResult.canceled) {
                if (canCancel) {
                    break;
                }
            } else {
                result.Value = dirResult.Value;
            }
        }

        Destroy(grid.gameObject);
        cursor.EnableReticules();
        dirCursor.gameObject.SetActive(false);
    }

    // selects an adjacent unit to the actor (provided they meet the rule), cancelable
    private IEnumerator SelectAdjacentUnitRoutine(Result<BattleUnit> result,
                BattleUnit actingUnit,
                Func<BattleUnit, bool> rule, 
                bool canCancel = true) {
        List<OrthoDir> dirs = new List<OrthoDir>();
        foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
            IntVector2 loc = actingUnit.location + dir.XY();
            BattleEvent doll = map.GetEventAt<BattleEvent>(map.LowestObjectLayer(), loc);
            if (doll != null && rule(doll.unit)) {
                dirs.Add(dir);
            }
        }
        if (dirs.Count > 0) {
            Result<OrthoDir> dirResult = new Result<OrthoDir>();
            yield return SelectTargetDirRoutine(dirResult, actingUnit, dirs, canCancel);
        } else {
            Debug.Assert(false, "No valid directions");
            result.Cancel();
        }
    }

    // selects a move location for the selected unit, might be canceled
    private IEnumerator SelectMoveLocationRoutine(Result<IntVector2> result, BattleUnit unit, bool canCancel=true) {
        cursor.gameObject.SetActive(true);
        cursor.GetComponent<MapEvent>().SetLocation(unit.location);

        SelectionGrid grid = SpawnSelectionGrid();
        int range = (int)unit.Get(StatTag.MOVE);
        Func<IntVector2, bool> rule = (IntVector2 loc) => {
            if (loc == unit.location) {
                return false;
            }
            return map.FindPath(unit.doll.GetComponent<CharaEvent>(), loc, range) != null;
        };
        grid.ConfigureNewGrid(map.size, rule);

        while (!result.finished) {
            Result<IntVector2> locResult = new Result<IntVector2>();
            yield return cursor.AwaitSelectionRoutine(locResult);
            if (locResult.canceled && canCancel) {
                result.Cancel();
            } else {
                if (rule(locResult.Value)) {
                    result.Value = locResult.Value;
                }
            }
        }

        cursor.gameObject.SetActive(false);
        Destroy(grid.gameObject);
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

    public void TargetCameraToLocation(IntVector2 loc) {
        TacticsCam.Instance().SetTargetLocation(loc);
    }

    private SelectionGrid SpawnSelectionGrid() {
        SelectionGrid grid = SelectionGrid.GetInstance();
        grid.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        return grid;
    }

    private void MoveCursorToDefaultUnit() {
        BattleUnit defaultHero = battle.GetFaction(Alignment.Hero).NextMoveableUnit();
        cursor.GetComponent<MapEvent>().SetLocation(defaultHero.location);
    }
}
