using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TilesetOverlay))]
public class TilesetOverlayEditor : Editor {

    public override void OnInspectorGUI() {
        //TilesetOverlay overlay = (TilesetOverlay)target;
        //if (GUILayout.Button("Save Changes")) {
        //    for (int y = 0; y < overlay.overlay.size.y; y += 1) {
        //        for (int x = 0; x < overlay.overlay.size.x; x += 1) {
        //            TileBase tile = overlay.overlay.GetTile(new Vector3Int(x, y, 0));
        //        }
        //    }
        //}

        //GUILayout.Space(25.0f);
        DrawDefaultInspector();
    }
}
