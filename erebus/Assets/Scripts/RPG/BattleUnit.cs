using System.Collections;
using System.Collections.Generic;

// representation of a unit in battle
public class BattleUnit {

    public Unit unit { get; private set; }
    public Battle battle { get; private set; }
    public Alignment align { get; private set; }
    public IntVector2 location { get; set; }
    public bool hasMovedThisTurn { get; private set; }

    public BattleEvent doll {
        get {
            return battle.controller.GetDollForUnit(this);
        }
    }

    // === INITIALIZATION ==========================================================================

    // we create battle units from three sources
    //  - unit, this is a keyed by what comes in from tiled and used to look up hero/enemy in db
    //  - battle, the parent battle creating this unit for
    //  - starting location, gleened from the tiled event usually
    public BattleUnit(Unit unit, Battle battle, IntVector2 location) {
        this.unit = unit;
        this.battle = battle;
        this.location = location;
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

    public float Get(StatTag tag) {
        return unit.stats.Get(tag);
    }

    public bool Is(StatTag tag) {
        return unit.stats.Is(tag);
    }

    // checks for deadness and dead-like conditions like petrification
    public bool IsDead() {
        return unit.stats.Get(StatTag.HP) <= 0;
    }
}
