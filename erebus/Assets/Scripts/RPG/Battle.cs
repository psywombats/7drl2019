using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A battle in progress. Responsible for all battle logic, state, and control flow. The actual
 * battle visual representation is contained in the BattleController. 
 * 
 * Flow for battles works like this:
 *  - A Tiled map is loaded that has the 'battle' property
 *  - A BattleController is created
 *  - The BattleController loads a serialized instance of this class via key
 *  - All the Tiled events participating in the battle register to the controller using the 'unit'
 *    key, and we then register them here
 */
[CreateAssetMenu(fileName = "Battle", menuName = "Data/RPG/Battle")]
public class Battle : ScriptableObject {
    
    public BattleController controller { get; private set; }
    
    private List<BattleUnit> units;

    // === INITIALIZATION ===

    public Battle() {
        this.units = new List<BattleUnit>();
    }

    // === BOOKKEEPING AND GETTERS ===

    public ICollection<BattleUnit> AllUnits() {
        return units;
    }

    public BattleUnit AddUnitFromKey(string unitKey) {
        Unit unit = Resources.Load<Unit>("Database/Units/" + unitKey);
        Debug.Assert(unit != null, "Unknown unit key " + unitKey);
        BattleUnit battleUnit = new BattleUnit(unit, this);
        AddUnit(battleUnit);
        return battleUnit;
    }

    private void AddUnit(BattleUnit unit) {
        units.Add(unit);
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
