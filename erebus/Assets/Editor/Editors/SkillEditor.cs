using UnityEditor;

[CustomEditor(typeof(Skill))]
public class SkillEditor : Editor {

    private PolymorphicFieldUtility targeterUtil;
    private PolymorphicFieldUtility effectorUtil;

    public void OnEnable() {
        targeterUtil = new PolymorphicFieldUtility(typeof(Targeter.TargeterParams),
            "Assets/Resources/Database/Targeters/" + ((Skill)target).name + "_targeter.asset");
        effectorUtil = new PolymorphicFieldUtility(typeof(Effector.EffectorParams),
            "Assets/Resources/Database/Effectors/" + ((Skill)target).name + "_effector.asset");
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Skill skill = (Skill)target;

        if (!serializedObject.FindProperty("targeter").hasMultipleDifferentValues) {
            skill.targeter = targeterUtil.DrawSelector(skill.targeter);
        }

        if (!serializedObject.FindProperty("effect").hasMultipleDifferentValues) {
            skill.effect = effectorUtil.DrawSelector(skill.effect);
        }
    }
}
