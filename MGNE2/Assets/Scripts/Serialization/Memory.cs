using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Memory {

    // switches
    public List<string> switchKeys;
    public List<bool> switchValues;

    // meta info
    public double savedAt;

    public Memory() {
        switchKeys = new List<string>();
        switchValues = new List<bool>();
        savedAt = Global.Instance().Memory.CurrentTimestamp();
    }
}
