using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SettingsMemory {

    public List<string> floatKeys;
    public List<float> floatValues;
    public List<string> boolKeys;
    public List<bool> boolValues;

    public SettingsMemory() {
        floatKeys = new List<string>();
        floatValues = new List<float>();
        boolKeys = new List<string>();
        boolValues = new List<bool>();
    }
}
