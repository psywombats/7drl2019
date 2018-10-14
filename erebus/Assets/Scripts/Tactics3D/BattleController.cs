using UnityEngine;
using System.Collections;

/**
 * Responsible for user input and rendering during a battle. Control flow is actually handled by
 * the Battle class.
 */
public class BattleController : MonoBehaviour {

    // properties required upon initializion
    public Battle battle { get; private set; }

    // all of these behaviors read + managed internally
    public Map map { get; private set; }

    // internal state

    public void SetupWithBattle(Battle battle) {
        this.battle = battle;
    }

    public void OnEnable() {
        this.map = FindObjectOfType<Map>();
    }
}
