using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Responsible for user input and rendering during a battle. Control flow is actually handled by
 * the Battle class.
 */
 [RequireComponent(typeof(Map))]
public class BattleController : MonoBehaviour {

    private static readonly IntVector2 CanceledLocation = new IntVector2(-1, -1);

    // properties required upon initializion
    public Battle battle { get; private set; }

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }

    // internal state
    private Dictionary<BattleUnit, BattleEvent> dolls;
    private BattleUnit actingUnit;
    private IntVector2 selectedLocation;

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

    // === STATE MACHINE ===========================================================================

    private void ResetActionState() {
        this.actingUnit = null;
        this.targetedMoveLocation = CanceledLocation;
    }

    // it's a discrete step in the human's turn, they should be able to undo up to this point
    public IEnumerator PlayNextHumanActionRoutine() {
        ResetActionState();

        while (actingUnit == null) {
            yield return SelectUnitRoutine();
            while (selectedLocation == CanceledLocation) {
                yield return SelectMoveLocationRoutine();
                if (actingUnit == null) {
                    break;
                }
            }
            
        }
    }

    // selects an allied unit, guarantees that this battle's selected unit will be non-null
    private IEnumerator SelectUnitRoutine() {
        // spawn the cursor
        while (this.actingUnit == null) {
            yield return null;
        }
    }

    // selects a move location for the selected unit, could potentially null out selected unit
    private IEnumerator SelectMoveLocationRoutine() {
        // spawn the cursor
        while (this.selectedLocation == CanceledLocation && this.actingUnit != null) {
            yield return null;
        }
    }
}
