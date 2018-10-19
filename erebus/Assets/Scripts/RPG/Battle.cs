using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<Alignment, BattleFaction> factions;

    // === INITIALIZATION ==========================================================================

    public Battle() {
        this.units = new List<BattleUnit>();
        this.factions = new Dictionary<Alignment, BattleFaction>();
    }

    // === BOOKKEEPING AND GETTERS =================================================================

    public ICollection<BattleUnit> AllUnits() {
        return units;
    }

    public IEnumerable<BattleUnit> UnitsByAlignment(Alignment align) {
        return units.Where(unit => (unit.align == align));
    }

    public BattleUnit AddUnitFromKey(string unitKey, IntVector2 startingLocation) {
        Unit unit = Global.Instance().Party.LookUpUnit(unitKey);
        Debug.Assert(unit != null, "Unknown unit key " + unitKey);
        BattleUnit battleUnit = new BattleUnit(unit, this, startingLocation);
        AddUnit(battleUnit);

        if (!factions.ContainsKey(battleUnit.align)) {
            factions[battleUnit.align] = new BattleFaction(this, battleUnit.align);
        }

        return battleUnit;
    }

    public List<BattleFaction> GetFactions() {
        return new List<BattleFaction>(factions.Values);
    }

    public BattleFaction GetFaction(Alignment align) {
        return this.factions[align];
    }

    private void AddUnit(BattleUnit unit) {
        units.Add(unit);
    }

    // === STATE MACHINE ===========================================================================

    // runs and executes this battle
    public IEnumerator BattleRoutine(BattleController controller) {
        this.controller = controller;
        while (true) {
            yield return NextRoundRoutine();
            if (CheckGameOver() != Alignment.None) {
                yield break;
            }
        }
    }
    
    // coroutine to play out a single round of combat
    private IEnumerator NextRoundRoutine() {
        yield return PlayTurnRoutine(Alignment.Hero);
        if (CheckGameOver() != Alignment.None) {
            yield break;
        }
        yield return PlayTurnRoutine(Alignment.Enemy);
        if (CheckGameOver() != Alignment.None) {
            yield break;
        }
    }

    // returns which alignment won the game, or Alignment.None if no one did
    private Alignment CheckGameOver() {
        foreach (BattleFaction faction in factions.Values) {
            if (faction.HasWon()) {
                return faction.align;
            } else if (faction.align == Alignment.Hero && faction.HasLost()) {
                return Alignment.Enemy;
            } else if (faction.align == Alignment.Enemy && faction.HasLost()) {
                return Alignment.Hero;
            }
        }
        return Alignment.None;
    }

    // responsible for changing ui state to this unit's turn, then 
    private IEnumerator PlayTurnRoutine(Alignment align) {
        if (factions[align] == null) {
            yield break;
        }
        yield return ResetForNewTurnRoutine(align);
        while (factions[align].HasUnitsLeftToAct()) {
            yield return PlayNextActionRoutine(align);
        }
    }

    private IEnumerator PlayNextActionRoutine(Alignment align) {
        switch (align) {
            case Alignment.Hero:
                yield return controller.PlayNextHumanActionRoutine();
                break;
            case Alignment.Enemy:
                // TODO: AI
                yield break;
            default:
                Debug.Assert(false, "bad align " + align);
                yield break;
        }
    }

    private IEnumerator ResetForNewTurnRoutine(Alignment align) {
        factions[align].ResetForNewTurn();
        yield break;
    }
}
