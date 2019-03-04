using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleEvent))]
public class BattleEventEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        BattleEvent battler = (BattleEvent)target;
        Unit unit = (Unit)EditorGUILayout.ObjectField("Unit", battler.unitSerialized, typeof(Unit), false);
        if (unit != battler.unitSerialized) {
            battler.PopulateWithUnitData(unit);
            EditorUtility.SetDirty(battler);
        }

        if (GUILayout.Button("Edit unit")) {
            Selection.activeObject = battler.unitSerialized;
        }
    }
}
