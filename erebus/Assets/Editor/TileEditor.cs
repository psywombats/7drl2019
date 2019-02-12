using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map2DTile))]
public class TileEditor : Editor {

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        Map2DTile targetTile = (Map2DTile)target;
        if (targetTile == null || targetTile.sprite == null) {
            return null;
        } else {
            Editor editor = CreateEditor(targetTile.sprite);
            string localDir = EditorUtils.LocalDirectoryFromPath(assetPath);
            string parentDir = localDir.Substring(0, localDir.LastIndexOf('/'));
            string name = EditorUtils.NameFromPath(assetPath);
            int lastIndex = name.LastIndexOf('[');
            string parentName = lastIndex > 0 ? name.Substring(0, lastIndex) : name;
            string path = parentDir + "/" + parentName + ".png";

            return editor.RenderStaticPreview(path, new Object[] { }, width, height);
        }
    }
}
