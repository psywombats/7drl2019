using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConfig : ScriptableObject {

    public AudioKeyData SoundEffects;
    public AudioKeyData BackgroundMusic;

    public static GlobalConfig GetInstance() {
        return Resources.Load<GlobalConfig>("Config");
    }
}
