using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapCamera))]
public class MapCameraEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MapCamera camera = (MapCamera)target;
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
