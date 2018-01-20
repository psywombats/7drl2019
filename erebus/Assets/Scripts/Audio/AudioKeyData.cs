using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioKeyData", menuName = "Data/AudioKeyData")]
public class AudioKeyData : ScriptableObject {

    public List<AudioKeyDataEntry> data;

}

[Serializable]
public class AudioKeyDataEntry {

    public string Key;
    public AudioClip Clip;

}
