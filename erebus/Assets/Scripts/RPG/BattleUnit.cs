using System.Collections;
using System.Collections.Generic;

// representation of a unit in battle
public class BattleUnit {

    public Unit Unit { get; private set; }
    public Battle Battle { get; private set; }
    public Alignment Align { get; private set; }
    public IntVector2 Location { get; private set; }

    public Doll Doll {
        get {
            return Battle.Controller.GetDollForUnit(this);
        }
    }

    public BattleUnit(Unit unit, Battle battle, Alignment align) {
        this.Unit = unit;
        this.Battle = battle;
        this.Align = align;

        Location = battle.GetStartingLocationFor(this);
    }
}
