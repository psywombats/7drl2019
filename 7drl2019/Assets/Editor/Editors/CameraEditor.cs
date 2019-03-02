using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapCamera))]
public class TacticsCameraEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        MapCamera camera = (MapCamera)target;
        
        if (GUILayout.Button("Center")) {
            if (Application.isPlaying) {
                camera.WarpToTarget();
            } else {
                camera.WarpToTarget(true);
            }
        }
    }
}
