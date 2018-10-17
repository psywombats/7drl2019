using System.Collections;
using System.Collections.Generic;

// representation of a unit in battle
public class BattleUnit {

    public Unit unit { get; private set; }
    public Battle battle { get; private set; }
    public Alignment align { get; private set; }
    public IntVector2 location { get; private set; }
    public bool hasMovedThisTurn { get; private set; }

    // === INITIALIZATION ==========================================================================

    public BattleEvent Doll {
        get {
            return battle.controller.GetDollForUnit(this);
        }
    }

    public BattleUnit(Unit unit, Battle battle) {
        this.unit = unit;
        this.battle = battle;

        location = battle.GetStartingLocationFor(this);
    }

    // given a doll from Tiled, copy over its relevant information
    public void CopyInfoFromDoll(BattleEvent doll) {
        this.location = doll.GetComponent<MapEvent3D>().Position;
    }

    // === STATE MACHINE ===========================================================================

    // perform action during faction's turn -- for humans, this involves menus, for enemies, AI
    public IEnumerator TakeAction() {
        hasMovedThisTurn = true;
        yield break;
    }

    // called at the beginning of this unit's faction's turn
    public void ResetForNewTurn() {

    }

    // === RPG =====================================================================================

    // checks for deadness and dead-like conditions like petrification
    public bool IsDead() {
        return unit.stats.Get(StatTag.HP) <= 0;
    }
}
