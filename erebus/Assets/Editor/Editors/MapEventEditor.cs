using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapEvent), true)]
public class MapEventEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUI.changed) {
            MapEvent ev = (MapEvent)target;
            ev.SetScreenPositionToMatchTilePosition();
            ev.SetDepth();
        }
    }
}
