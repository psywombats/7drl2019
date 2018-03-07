using System.Collections;
using System.Collections.Generic;

// representation of a unit in battle
public class BattleUnit {

    public Unit Unit { get; private set; }
    public Battle Battle { get; private set; }
    public Doll doll { get; private set; }
    
    public Alignment Align {
        get {
            return Unit.Align;
        }
    }

    public BattleUnit(Unit unit, Battle battle) {
        this.Unit = unit;
        this.Battle = battle;
    }
}
