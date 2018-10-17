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
    public Battle battle { get; private set; }

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }

    // internal state
    private Dictionary<BattleUnit, BattleEvent> dolls;

    // === INITIALIZATION ===

    public BattleController() {
        dolls = new Dictionary<BattleUnit, BattleEvent>();
    }

    // this should take a battle memory at some point
    public void Setup(string battleKey) {
        this.battle = Resources.Load<Battle>("Database/Battles/" + battleKey);
        battle.RegisterController(this);
        Debug.Assert(this.battle != null, "Unknown battle key " + battleKey);
    }

    // === GETTERS AND BOOKKEEPING ===

    public BattleEvent GetDollForUnit(BattleUnit unit) {
        return dolls[unit];
    }

    public void AddUnitFromTiledEvent(BattleEvent doll, string unitKey) {
        BattleUnit newUnit = battle.AddUnitFromKey(unitKey);
        newUnit.CopyInfoFromDoll(doll);
        doll.Setup(this, newUnit);
        dolls[newUnit] = doll;
    }
}
