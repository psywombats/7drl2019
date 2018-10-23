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
    private BattleUnit actingUnit;
    private IntVector2 selectionPosition;
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

    private void ResetActionState() {
        this.actingUnit = null;
        this.selectionPosition = Cursor.CanceledLocation;
    }

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
        ResetActionState();

        MoveCursorToDefaultUnit();

        while (actingUnit == null) {
            yield return SelectUnitRoutine();
            while (selectionPosition == Cursor.CanceledLocation) {
                yield return SelectMoveLocationRoutine();
                if (selectionPosition == Cursor.CanceledLocation) {
                    break;
                }
                actingUnit.location = selectionPosition;
                yield return actingUnit.doll.GetComponent<CharaEvent>().PathToRoutine(selectionPosition);
            }

            // TODO: remove this nonsense
            yield return SelectAdjacentUnitRoutine((BattleUnit unit) => {
                return unit.align == Alignment.Enemy;
            });
            if (selectionPosition == Cursor.CanceledLocation) {
                break;
            }
            BattleUnit targetUnit = map.GetEventAt<BattleEvent>(map.LowestObjectLayer(), selectionPosition).unit;
            targetUnit.doll.GetComponent<CharaEvent>().FaceToward(actingUnit.location);

            yield return Global.Instance().Maps.ActiveDuelMap.EnterMapRoutine(actingUnit.doll, targetUnit.doll);
            yield return new WaitForSeconds(2.0f);

            actingUnit.MarkActionTaken();
            yield return Global.Instance().Maps.ActiveDuelMap.ExitMapRoutine();
            yield return actingUnit.doll.PostActionRoutine();
        }
    }

    // selects an allied unit, guarantees that this battle's selected unit will be non-null 
    private IEnumerator SelectUnitRoutine() {
        cursor.gameObject.SetActive(true);
        cursor.Configure((IntVector2 loc) => {
            BattleUnit unit = GetUnitAt(loc);
            if (unit != null && unit.align == Alignment.Hero) {
                actingUnit = unit;
            }
        });

        while (actingUnit == null) {
            yield return cursor.AwaitSelectionRoutine();
        }
        cursor.gameObject.SetActive(false);
    }

    // selects a square to be targeted by the acting unit, might be canceled
    private IEnumerator SelectTargetDirRoutine(HashSet<OrthoDir> allowedDirections) { 
        dirCursor.gameObject.SetActive(true);
        cursor.DisableReticules();

        SelectionGrid grid = SpawnSelectionGrid();
        grid.ConfigureNewGrid(new IntVector2(3, 3), (IntVector2 loc) => {
            return (loc.x + loc.y) % 2 == 1;
        });
        grid.GetComponent<MapEvent>().Position = actingUnit.location - new IntVector2(1, 1);
        grid.GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();

        selectionPosition = Cursor.CanceledLocation;
        dirCursor.Configure(actingUnit.doll.GetComponent<CharaEvent>(), (IntVector2 loc) => {
            selectionPosition = loc;
        });
        dirCursor.dir = allowedDirections.ElementAt(0);
        do {
            yield return dirCursor.AwaitSelectionRoutine();
        } while (selectionPosition != Cursor.CanceledLocation
                && !allowedDirections.Contains(OrthoDirExtensions.DirectionOf(selectionPosition - actingUnit.location)));

        Destroy(grid.gameObject);
        cursor.EnableReticules();
        dirCursor.gameObject.SetActive(false);
    }

    // selects an adjacent unit to the actor (provided they meet the rule), cancelable
    private IEnumerator SelectAdjacentUnitRoutine(Func<BattleUnit, bool> rule) {
        HashSet<OrthoDir> dirs = new HashSet<OrthoDir>();
        foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
            IntVector2 loc = actingUnit.location + dir.XY();
            BattleEvent doll = map.GetEventAt<BattleEvent>(map.LowestObjectLayer(), loc);
            if (doll != null && rule(doll.unit)) {
                dirs.Add(dir);
            }
        }
        if (dirs.Count > 0) {
            yield return SelectTargetDirRoutine(dirs);
        } else {
            selectionPosition = Cursor.CanceledLocation;
            yield return null;
        }
    }

    // selects a move location for the selected unit, might be canceled
    private IEnumerator SelectMoveLocationRoutine() {
        cursor.gameObject.SetActive(true);
        cursor.GetComponent<MapEvent>().SetLocation(actingUnit.location);
        cursor.Configure((IntVector2 loc) => {
            this.selectionPosition = loc;
        });
        
        SelectionGrid grid = SpawnSelectionGrid();
        int range = (int)actingUnit.Get(StatTag.MOVE);
        Func<IntVector2, bool> rule = (IntVector2 loc) => {
            if (loc == actingUnit.location) {
                return false;
            }
            return map.FindPath(actingUnit.doll.GetComponent<CharaEvent>(), loc, range) != null;
        };
        grid.ConfigureNewGrid(map.size, rule);

        do {
            yield return cursor.AwaitSelectionRoutine();
        } while (selectionPosition != Cursor.CanceledLocation && !rule(selectionPosition));

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
