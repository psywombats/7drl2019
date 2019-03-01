using UnityEngine;
using System.Collections;

/**
 * A big bad brain that dictates how enemy units take their turns. It's a scriptable object that
 * serializes along with battles, the idea being that "ai easymode" or "ai brutal" can be drag-
 * dropped onto battle instances.
 */
[CreateAssetMenu(fileName = "AIController", menuName = "Data/RPG/AI")]
public class AIController : ScriptableObject {

    private Battle battle;
    private BattleController controller;

    // set up internal state at the start of a battle
    public void ConfigureForBattle(Battle battle) {
        this.battle = battle;
        this.controller = battle.controller;
    }

    // called repeatedly by the battle while ai units still have moves left
    public IEnumerator PlayNextAIActionRoutine() {
        BattleUnit actor = battle.GetFaction(Alignment.Enemy).NextMoveableUnit();
        controller.TargetCameraToLocation(actor.location);
        yield return new WaitForSeconds(0.8f);

        // TODO: the ai
    }
}
