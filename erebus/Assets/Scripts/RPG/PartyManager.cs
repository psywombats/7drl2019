using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MemoryPopulater {

    private List<Unit> knownUnits;

    public PartyManager() {
        Global.Instance().Memory.RegisterMemoryPopulater(this);
        knownUnits = new List<Unit>();
    }

    public void PopulateFromMemory(Memory memory) {
        knownUnits.Clear();
        foreach (UnitMemory unitMemory in memory.rpg.KnownUnits) {
            knownUnits.Add(new Unit(unitMemory));
        }
    }

    public void PopulateMemory(Memory memory) {
        throw new System.NotImplementedException();
    }
}
