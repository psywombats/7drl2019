using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileImporter : AssetPostprocessor {


    public void OnPreprocessTexture() {
        if (!assetPath.Contains("Tilesets") || assetPath.Contains("_Tiles")) {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        IntVector2 textureSize = EditorUtils.GetPreprocessedImageSize(importer);
        string name = EditorUtils.NameFromPath(assetPath);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = Map.TileSizePx;
        importer.spritePivot = new Vector2(Map.TileSizePx / 2, Map.TileSizePx / 2);

        List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
        for (int y = 0; y < textureSize.y / Map.TileSizePx; y += 1) {
            for (int x = 0; x < textureSize.x / Map.TileSizePx; x += 1) {
                SpriteMetaData data = new SpriteMetaData();
                data.rect = new Rect(
                    x * Map.TileSizePx,
                    y * Map.TileSizePx,
                    Map.TileSizePx,
                    Map.TileSizePx);
                data.alignment = (int)SpriteAlignment.Custom;
                data.border = new Vector4(0, 0, 0, 0);
                data.name = name + "[" + x + "][" + y + "]";
                data.pivot = new Vector2(0.5f, 0.5f);
                spritesheet.Add(data);
            }
        }
        importer.spritesheet = spritesheet.ToArray();
    }

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string assetPath in importedAssets) {
            if (assetPath.Contains("Tilesets") && !assetPath.Contains("_Tiles")) {
                CreateTiles(assetPath);
            }
        }
    }

    public static void CreateTiles(string assetPath) {
        string localDirectory = EditorUtils.LocalDirectoryFromPath(assetPath);
        string name = EditorUtils.NameFromPath(assetPath);
        string tilesDirName = name + "_Tiles";
        string tilesDirPath = localDirectory + "/" + tilesDirName;
        if (!AssetDatabase.IsValidFolder(localDirectory + "/" + tilesDirName)) {
            AssetDatabase.CreateFolder(localDirectory, tilesDirName);
        }

        Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        foreach (Object spriteObject in spriteObjects) {
            if (!spriteObject.GetType().IsAssignableFrom(typeof(Sprite))) {
                continue;
            }
            Sprite sprite = (Sprite)spriteObject;
            string spritePath = tilesDirPath + "/" + sprite.name + ".asset";
            Map2DTile tile = AssetDatabase.LoadAssetAtPath<Map2DTile>(spritePath);
            if (tile == null) {
                tile = ScriptableObject.CreateInstance<Map2DTile>();
                AssetDatabase.CreateAsset(tile, spritePath);
            }
            tile.sprite = sprite;
            tile.name = sprite.name;
        }
        AssetDatabase.SaveAssets();
    }
}
