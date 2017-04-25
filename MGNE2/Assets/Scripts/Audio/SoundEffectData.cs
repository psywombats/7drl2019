using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectData", menuName = "Data/SoundEffectData")]
public class SoundEffectData : ScriptableObject {

    public List<SoundEffectDataEntry> data;

}

[Serializable]
public class SoundEffectDataEntry {

    public string Key;
    public AudioClip Clip;

}
