using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransitionIndexData", menuName = "Data/IndexDatabase")]
public class IndexDatabase : ScriptableObject {

    public TransitionIndexData Transitions;
    public FadeIndexData Fades;
    public SoundEffectIndexData SFX;
    public BGMIndexData BGM;
    public BackgroundIndexData Backgrounds;
    public CharaIndexData Charas;

    public static IndexDatabase Instance() {
        return Resources.Load<IndexDatabase>("Database/Database");
    }
}
