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
        throw new System.NotImplementedException();
    }

    public void PopulateMemory(Memory memory) {
        throw new System.NotImplementedException();
    }
}
