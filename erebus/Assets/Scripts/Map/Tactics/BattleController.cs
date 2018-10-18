using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/**
 * Responsible for user input and rendering during a battle. Control flow is actually handled by
 * the Battle class.
 */
[RequireComponent(typeof(Map))]
public class BattleController : MonoBehaviour {

    // properties required upon initializion
    public Battle battle;

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }

    // internal state
    private Dictionary<BattleUnit, BattleEvent> dolls;
    private BattleUnit actingUnit;
    private IntVector2 selectionPosition;
    private Cursor cursor;

    // === INITIALIZATION ==========================================================================

    public BattleController() {
        dolls = new Dictionary<BattleUnit, BattleEvent>();
    }

    public void Start() {
        cursor = Cursor.GetInstance();
        cursor.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        cursor.gameObject.SetActive(false);
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
        BattleUnit newUnit = battle.AddUnitFromKey(unitKey);
        newUnit.CopyInfoFromDoll(doll);
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

    // selects a move location for the selected unit, might be canceled
    private IEnumerator SelectMoveLocationRoutine() {
        cursor.gameObject.SetActive(true);
        cursor.GetComponent<MapEvent>().SetLocation(actingUnit.location);
        cursor.Configure((IntVector2 loc) => {
            this.selectionPosition = loc;
        });

        SelectionGrid grid = SpawnSelectionGrid();
        int range = (int)actingUnit.Get(StatTag.MOVE);
        grid.ConfigureNewGrid(map.size, (IntVector2 loc) => {
            if (loc == actingUnit.location) {
                return false;
            }
            return map.FindPath(actingUnit.doll.GetComponent<CharaEvent>(), loc, range) != null;
        });

        yield return cursor.AwaitSelectionRoutine();

        cursor.gameObject.SetActive(false);
        Destroy(grid.gameObject);
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

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
