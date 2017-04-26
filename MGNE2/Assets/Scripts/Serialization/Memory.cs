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

    public Memory(MemoryManager manager) {

        // switches
        switchKeys = new List<string>();
        switchValues = new List<bool>();

        // other state
        mapName = Global.Instance().Maps.ActiveMap.InternalName;
        position = Global.Instance().Maps.Avatar.GetComponent<MapEvent>().Position;
        facing = Global.Instance().Maps.Avatar.GetComponent<CharaEvent>().Facing;
        bgmKey = Global.Instance().Audio.CurrentBGMKey;

        // meta info
        savedAt = manager.CurrentTimestamp();
        saveVersion = MemoryManager.CurrentSaveVersion;
    }
}
