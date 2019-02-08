using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Memory {

    // variables
    public SerialDictionary<string, int> variables;
    public SerialDictionary<string, bool> switches;

    // other state
    public string mapName;
    public IntVector2 position;
    public OrthoDir facing;
    public string bgmKey;

    // meta info
    public int saveVersion;
    public double savedAt;
    public string base64ScreenshotPNG;

    // rpg
    public RPGMemory rpg;
}
