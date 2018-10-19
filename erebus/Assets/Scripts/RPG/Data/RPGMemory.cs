using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RPGMemory {

    public List<Unit> knownUnits;
    public List<string> partyUnitKeys;

    public RPGMemory() {
        knownUnits = new List<Unit>();
        partyUnitKeys = new List<string>();
    }
}
