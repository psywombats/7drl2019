using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Memory {

    // switches
    public List<string> switchKeys;
    public List<bool> switchValues;

    // meta info
    public int saveVersion;
    public double savedAt;

    public Memory(MemoryManager manager) {
        switchKeys = new List<string>();
        switchValues = new List<bool>();
        savedAt = manager.CurrentTimestamp();
        saveVersion = MemoryManager.CurrentSaveVersion;
    }
}
