using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleEvent))]
public class BattleEventEditor : Editor {

    public override void OnInspectorGUI() {
        BattleEvent battler = (BattleEvent)target;
        Unit unit = (Unit)EditorGUILayout.ObjectField("Unit", battler.unitData, typeof(Unit), false);
        if (unit != battler.unitData) {
            battler.PopulateWithUnitData(unit);
            EditorUtility.SetDirty(battler);
        }

        if (GUILayout.Button("Edit unit")) {
            Selection.activeObject = battler.unitData;
        }
    }
}
