using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TacticsCam))]
public class TacticsCameraEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        TacticsCam camera = (TacticsCam)target;
        if (GUILayout.Button("Center")) {
            camera.WarpToTarget();
        }
    }
}
