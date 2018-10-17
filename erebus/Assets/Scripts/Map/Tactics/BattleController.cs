using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // === INITIALIZATION ==========================================================================

    public BattleController() {
        dolls = new Dictionary<BattleUnit, BattleEvent>();
    }

    // this should take a battle memory at some point
    public void Setup(string battleKey) {
        this.battle = Resources.Load<Battle>("Database/Battles/" + battleKey);
        battle.RegisterController(this);
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
        this.selectionPosition = Cursor.CanceledPosition;
    }

    // it's a discrete step in the human's turn, they should be able to undo up to this point
    public IEnumerator PlayNextHumanActionRoutine() {
        ResetActionState();

        while (actingUnit == null) {
            yield return SelectUnitRoutine();
            while (selectionPosition == Cursor.CanceledPosition) {
                yield return SelectMoveLocationRoutine();
                if (actingUnit == null) {
                    break;
                }
            }
            
        }
    }

    // selects an allied unit, guarantees that this battle's selected unit will be non-null 
    private IEnumerator SelectUnitRoutine() {
        BattleUnit defaultHero = battle.GetFaction(Alignment.Hero).NextMoveableUnit();
        Cursor cursor = SpawnCursor();
        cursor.GetComponent<MapEvent>().SetLocation(defaultHero.position);
        while (actingUnit == null) {
            cursor.Configure((IntVector2 pos) => {
                BattleUnit unit = GetUnitAt(pos);
                if (unit != null && unit.align == Alignment.Hero) {
                    actingUnit = unit;
                }
            });
            yield return cursor.AwaitSelectionRoutine();
        }
        Destroy(cursor);
    }

    // selects a move location for the selected unit, could potentially null out selected unit
    private IEnumerator SelectMoveLocationRoutine() {
        // spawn the cursor
        while (this.selectionPosition == Cursor.CanceledPosition && this.actingUnit != null) {
            yield return null;
        }
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

    private SelectionGrid SpawnSelectionGrid() {
        SelectionGrid grid = SelectionGrid.GetInstance();
        grid.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        return grid;
    }

    private Cursor SpawnCursor() {
        Cursor cursor = Cursor.GetInstance();
        cursor.gameObject.transform.parent = GetComponent<Map>().LowestObjectLayer().transform;
        return cursor;
    }
}
