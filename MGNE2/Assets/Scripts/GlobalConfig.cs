using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConfig : ScriptableObject {

    public SoundEffectData SoundEffects;

    public static GlobalConfig GetInstance() {
        return Resources.Load<GlobalConfig>("Config");
    }
}
