using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapCamera2D))]
public class MapCameraEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MapCamera2D camera = (MapCamera2D)target;
        if (GUILayout.Button("Attach and Center")) {
            AvatarEvent avatar = GameObject.FindObjectOfType<AvatarEvent>();
            if (avatar != null) {
                camera.Target = avatar.GetComponent<MapEvent>();
                camera.ManualUpdate();
            } else {
                Debug.LogError("No avatar could be found in the scene");
            }
        }
    }
}
