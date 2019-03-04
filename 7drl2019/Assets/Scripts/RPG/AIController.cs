using UnityEngine;
using System.Collections;

public class AIController {

    public BattleUnit unit { get; private set; }
    public BattleEvent battler { get { return unit.battler; } }
    public BattleUnit pc { get { return unit.battle.pc; } }

    public AIController(BattleUnit unit) {
        this.unit = unit;
    }

    public IEnumerator TakeTurnAction() {
        return battler.StepOrAttackAction(battler.GetComponent<MapEvent>().DirectionTo(pc.location));
    }
}
