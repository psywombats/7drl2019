using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SoundEffectIndexData", menuName = "Data/VN/SoundEffectIndexData")]
public class SoundEffectIndexData : GenericIndex<SoundEffectData> {

}

[Serializable]
public class SoundEffectData : GenericDataObject {

    public AudioClip clip;

}
