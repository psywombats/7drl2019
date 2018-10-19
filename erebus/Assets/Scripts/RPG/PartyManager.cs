using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Manages the party and other persistent units across the game.
 */
public class PartyManager : MonoBehaviour, MemoryPopulater {

    // unit key to unit
    private Dictionary<string, Unit> knownUnits;
    private List<Unit> partyMembers;

    public PartyManager() {
        Global.Instance().Memory.RegisterMemoryPopulater(this);
        knownUnits = new Dictionary<string, Unit>();
        partyMembers = new List<Unit>();
    }

    // either loads a unit from the db or from our store
    public Unit LookUpUnit(string unitKey) {
        if (!knownUnits.ContainsKey(unitKey)) {
            Unit unit = Resources.Load<Unit>("Database/Units/" + unitKey);
            if (!unit.unique) {
                knownUnits[unitKey] = unit;
            }
            return unit;
        } else {
            return knownUnits[unitKey];
        }
    }

    public void PopulateFromMemory(Memory memory) {
        knownUnits = new Dictionary<string, Unit>();
        foreach (Unit unit in memory.rpg.knownUnits) {
            knownUnits[unit.name] = unit;
        }
        partyMembers = new List<Unit>();
        foreach (string key in memory.rpg.partyUnitKeys) {
            partyMembers.Add(knownUnits[key]);
        }
    }

    public void PopulateMemory(Memory memory) {
        memory.rpg = new RPGMemory();
        memory.rpg.knownUnits = new List<Unit>(knownUnits.Values);
        memory.rpg.partyUnitKeys = new List<string>(partyMembers.Select((Unit unit) => { return unit.name; }));
    }
}
