using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle {

    public HashSet<BattleUnit> Units { get; private set; }

    private Dictionary<Alignment, HashSet<BattleUnit>> unitsByAlignment;

    public Battle() {
        Units = new HashSet<BattleUnit>();
        unitsByAlignment = new Dictionary<Alignment, HashSet<BattleUnit>>();
    }

    public void AddUnit(BattleUnit unit) {
        Debug.Assert(!Units.Contains(unit));
        if (!unitsByAlignment.ContainsKey(unit.Align)) {
            unitsByAlignment[unit.Align] = new HashSet<BattleUnit>();
        }
        unitsByAlignment[unit.Align].Add(unit);
    }

    public void AddUnit(Unit unit) {
        BattleUnit battleUnit = new BattleUnit(unit, this);
        AddUnit(battleUnit);
    }

    private void RemoveUnit(BattleUnit unit) {
        Debug.Assert(Units.Contains(unit));
        unitsByAlignment[unit.Align].Remove(unit);
        Units.Add(unit);
    }

    public HashSet<BattleUnit> unitsForAlignment(Alignment align) {
        return unitsByAlignment[align];
    }
}
