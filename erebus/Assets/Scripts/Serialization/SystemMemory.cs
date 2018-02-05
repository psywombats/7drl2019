using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SystemMemory {

    public List<string> maxSeenCommandsKeys;
    public List<int> maxSeenCommandsValues;
    public int totalPlaySeconds;
    public int lastSlotSaved;

    public SystemMemory() {
        maxSeenCommandsKeys = new List<string>();
        maxSeenCommandsValues = new List<int>();
        lastSlotSaved = -1;
    }
}
