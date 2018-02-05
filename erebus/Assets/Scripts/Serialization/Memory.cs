using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Memory {

    // variables
    public List<string> variableKeys;
    public List<int> variableValues;
    public List<string> switchKeys;
    public List<bool> switchValues;

    // scene data
    public ScreenMemory screen;

    // other state
    public string mapName;
    public IntVector2 position;
    public OrthoDir facing;
    public string bgmKey;

    // meta info
    public int saveVersion;
    public double savedAt;
    public string base64ScreenshotPNG;

}
