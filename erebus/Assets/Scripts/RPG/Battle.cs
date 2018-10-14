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
    
    public BattleController Controller { get; private set; }

    private Dictionary<Alignment, HashSet<BattleUnit>> unitsByAlignment;
    private HashSet<BattleUnit> units;

    // === INITIALIZATION ===

    public Battle(BattleController controller) {
        this.Controller = controller;
        this.units = new HashSet<BattleUnit>();
        this.unitsByAlignment = new Dictionary<Alignment, HashSet<BattleUnit>>();
    }

    // === BOOKKEEPING AND GETTERS ===

    public ICollection<BattleUnit> AllUnits() {
        return units;
    }

    public void AddUnit(BattleUnit unit) {
        Debug.Assert(!units.Contains(unit));
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
        Debug.Assert(units.Contains(unit));
        unitsByAlignment[unit.Align].Remove(unit);
        units.Add(unit);
    }

    public HashSet<BattleUnit> UnitsForAlignment(Alignment align) {
        return unitsByAlignment[align];
    }

    // === BATTLE INITIALIZATION ===

    public IntVector2 GetStartingLocationFor(BattleUnit unit) {
        // TODO: GetStartingLocationFor
        return new IntVector2(2, 9);
    }

    // === STATE MACHINE ===

    public void StartBattle() {

    }
}
