using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        MapGenerator generator = (MapGenerator)target;
        
        if (GUILayout.Button("Regenerate")) {
            generator.GenerateMesh();
        }
    }
}
