using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleController))]
public class BattleControllerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        BattleController controller = (BattleController)target;
        if (Application.isPlaying && !controller.started) {
            if (GUILayout.Button("Start Battle")) {
                controller.StartCoroutine(controller.BattleRoutine());
            }
        } else {
            GUILayout.Label("Enter play mode to test battle");
        }
    }
}
