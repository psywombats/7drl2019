using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Responsible for user input and rendering during a battle. Control flow is actually handled by
 * the Battle class.
 */
public class BattleController : MonoBehaviour {

    // properties required upon initializion
    private Battle battle;
    public Battle Battle { get { return battle; } private set { battle = value; } }

    // all of these behaviors read + managed internally
    private Map map;
    public Map Map { get { return map; } private set { map = value; } }

    // internal state
    private Dictionary<BattleUnit, Doll> dolls;

    // === INITIALIZATION ===

    // this should take a battle memory at some point
    public void Setup() {
        this.battle = new Battle(this);

        dolls = new Dictionary<BattleUnit, Doll>();
        foreach (BattleUnit unit in battle.AllUnits()) {
            Doll doll = Doll.Instantiate(this, unit);
            dolls[unit] = doll;
        }
    }

    public void OnEnable() {
        this.map = FindObjectOfType<Map>();
    }

    // === GETTERS AND BOOKKEEPING ===

    public Doll GetDollForUnit(BattleUnit unit) {
        return dolls[unit];
    }
}
