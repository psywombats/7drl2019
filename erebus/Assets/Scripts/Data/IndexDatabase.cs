using UnityEngine;

[CreateAssetMenu(fileName = "IndexDatabase", menuName = "Data/IndexDatabase")]
public class IndexDatabase : ScriptableObject {

    public TransitionIndexData Transitions;
    public FadeIndexData Fades;
    public SoundEffectIndexData SFX;
    public BGMIndexData BGM;

    public static IndexDatabase Instance() {
        return Resources.Load<IndexDatabase>("Database/Database");
    }
}
