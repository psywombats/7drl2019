using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConfig : ScriptableObject {

    public BGMIndexData SoundEffects;
    public SoundEffectIndexData BackgroundMusic;

    public static GlobalConfig GetInstance() {
        return Resources.Load<GlobalConfig>("Config");
    }
}
