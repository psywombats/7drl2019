using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Memory {

    // switches
    public List<string> switchKeys;
    public List<bool> switchValues;

    // other state
    public string mapName;
    public IntVector2 position;
    public OrthoDir facing;
    public string bgmKey;

    // meta info
    public int saveVersion;
    public double savedAt;

}
