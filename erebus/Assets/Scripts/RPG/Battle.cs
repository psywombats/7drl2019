using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A battle in progress. Responsible for all battle logic, state, and control flow. The actual
 * battle visual representation is contained in the BattleController.
 * 
 * Eventually this should be created from a battle memory or similar.
 */
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

    public void AddUnit(Unit unit, Alignment align) {
        BattleUnit battleUnit = new BattleUnit(unit, this, align);
        AddUnit(battleUnit);
    }

    private void RemoveUnit(BattleUnit unit) {
        Debug.Assert(Units.Contains(unit));
        unitsByAlignment[unit.Align].Remove(unit);
        Units.Add(unit);
    }

    public HashSet<BattleUnit> UnitsForAlignment(Alignment align) {
        return unitsByAlignment[align];
    }
}
