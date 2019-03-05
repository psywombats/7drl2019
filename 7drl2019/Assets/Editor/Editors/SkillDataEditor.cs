using UnityEditor;

[CustomEditor(typeof(SkillData))]
public class SkillEditor : Editor {

    private PolymorphicFieldUtility targeterUtil;
    private PolymorphicFieldUtility effectorUtil;

    public void OnEnable() {
        targeterUtil = new PolymorphicFieldUtility(typeof(Targeter),
            "Assets/Resources/Database/Targeters/" + ((SkillData)target).name + "_targeter.asset");
        effectorUtil = new PolymorphicFieldUtility(typeof(Effector),
            "Assets/Resources/Database/Effectors/" + ((SkillData)target).name + "_effector.asset");
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SkillData skill = (SkillData)target;

        if (!serializedObject.FindProperty("targeter").hasMultipleDifferentValues) {
            skill.targeter = targeterUtil.DrawSelector(skill.targeter);
        }

        if (!serializedObject.FindProperty("effect").hasMultipleDifferentValues) {
            skill.effect = effectorUtil.DrawSelector(skill.effect);
        }
    }
}
