using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleController))]
public class BattleControllerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        BattleController controller = (BattleController)target;
        if (Application.isPlaying) {
            if (GUILayout.Button("Start Battle")) {
                controller.StartCoroutine(controller.battle.BattleRoutine(controller));
            }
        } else {
            GUILayout.Label("Enter play mode to test battle");
        }

        if (controller.battle != null) {
            GUILayout.Label("Battle status:");
            foreach (BattleFaction faction in controller.battle.GetFactions()) {
                GUILayout.Label("  " + faction);
            }
        } else {
            controller.battle = new Battle();
        }
    }
}
