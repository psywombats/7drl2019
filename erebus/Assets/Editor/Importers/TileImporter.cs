using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileImporter : AssetPostprocessor {

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string assetPath in importedAssets) {
            if (assetPath.Contains("Tilesets") && assetPath.Contains(".png") && !assetPath.Contains("3D")) {
                CreateTiles(assetPath);
                AssetDatabase.SaveAssets();
                CreatePalette(assetPath);
            }
            if (assetPath.Contains("Palettes") && assetPath.EndsWith(".prefab")) {
                PopulatePalette(assetPath);
            }
        }
    }

    public void OnPreprocessTexture() {
        if (!assetPath.Contains("Tilesets") || assetPath.Contains("_Tiles")) {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        Vector2Int textureSize = EditorUtils.GetPreprocessedImageSize(importer);
        string name = EditorUtils.NameFromPath(assetPath);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = Map.UnityUnitScale;
        importer.spritePivot = new Vector2(Map.TileSizePx / 2, Map.TileSizePx / 2);

        List<SpriteMetaData> newSheet = new List<SpriteMetaData>();
        for (int y = 0; y < textureSize.y / Map.TileSizePx; y += 1) {
            for (int x = 0; x < textureSize.x / Map.TileSizePx; x += 1) {
                int index = y * (textureSize.y / Map.TileSizePx) + x;
                SpriteMetaData data = new SpriteMetaData();
                data.rect = new Rect(
                    x * Map.TileSizePx,
                    (textureSize.y / Map.TileSizePx - y - 1) * Map.TileSizePx,
                    Map.TileSizePx,
                    Map.TileSizePx);
                data.alignment = (int)SpriteAlignment.Custom;
                data.border = new Vector4(0, 0, 0, 0);
                data.name = NameForTile(name, x, y);
                data.pivot = new Vector2(0.5f, 0.5f);
                newSheet.Add(data);
            }
        }
        importer.spritesheet = newSheet.ToArray();
    }

    public static void CreateTiles(string assetPath) {
        string localDirectory = EditorUtils.LocalDirectoryFromPath(assetPath);
        string name = EditorUtils.NameFromPath(assetPath);
        string genericPath = localDirectory + "/Tiles";
        string tilesDirPath = genericPath + "/" + name;
        if (!AssetDatabase.IsValidFolder(genericPath)) {
            AssetDatabase.CreateFolder(localDirectory, "Tiles");
        }
        if (!AssetDatabase.IsValidFolder(tilesDirPath)) {
            AssetDatabase.CreateFolder(genericPath, name);
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
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.Sprite;
            tile.transform = Matrix4x4.identity;
        }
    }

    public static void CreatePalette(string assetPath) {
        string localDirectory = EditorUtils.LocalDirectoryFromPath(assetPath);
        string name = EditorUtils.NameFromPath(assetPath);
        string palettesDirPath = localDirectory + "/Palettes";
        string palettePath = palettesDirPath + "/" + name + ".prefab";
        if (!AssetDatabase.IsValidFolder(palettesDirPath)) {
            AssetDatabase.CreateFolder(localDirectory, "Palettes");
        }

        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(palettePath);
        Tilemap tilemap;
        if (palette == null) {
            palette = new GameObject();
            Grid grid = palette.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSize = new Vector3(Map.UnityUnitScale, Map.UnityUnitScale, 0.0f);
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            GameObject layer = new GameObject("Layer1");
            layer.transform.parent = palette.transform;
            tilemap = layer.AddComponent<Tilemap>();
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0.0f);

            TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.BottomLeft;
            renderer.mode = TilemapRenderer.Mode.Chunk;

            PrefabUtility.SaveAsPrefabAsset(palette, palettePath);
            Object.DestroyImmediate(palette);
            AssetDatabase.SaveAssets();
        } else {
            AssetDatabase.ImportAsset(palettePath, ImportAssetOptions.ForceUpdate);
        }
    }

    public static void PopulatePalette(string assetPath) {
        bool dirty = false;

        GameObject paletteObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GridPalette settings = null;
        Tilemap map = paletteObject.GetComponentInChildren<Tilemap>();
        foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(assetPath)) {
            if (obj is GridPalette) {
                settings = (GridPalette)obj;
            }
        }
        if (settings == null) {
            settings = ScriptableObject.CreateInstance<GridPalette>();
            settings.name = "Palette Settings";
            settings.cellSizing = GridPalette.CellSizing.Automatic;
            AssetDatabase.AddObjectToAsset(settings, paletteObject);
            dirty = true;
        }

        string name = EditorUtils.NameFromPath(assetPath);
        string localDirectory = EditorUtils.LocalDirectoryFromPath(assetPath);
        string pngDirectory = localDirectory.Substring(0, localDirectory.LastIndexOf('/'));
        string tileDirectory = pngDirectory + "/Tiles/" + name;
        string texturePath = pngDirectory + "/" + name + ".png";
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        for (int y = 0; y < tex.height / Map.TileSizePx; y += 1) {
            for (int x = 0; x < tex.width / Map.TileSizePx; x += 1) {
                Vector3Int pos = new Vector3Int(x, (tex.height / Map.TileSizePx - y - 1), 0);
                TileBase existingTile = map.GetTile<TileBase>(pos);
                
                string tileName = NameForTile(name, x, y);
                Map2DTile newTile = AssetDatabase.LoadAssetAtPath<Map2DTile>(tileDirectory + "/" + tileName + ".asset");
                
                if (!newTile.EqualsTile(existingTile)) {
                    dirty = true;
                    map.SetTile(pos, newTile);
                }
            }
        }

        if (dirty) {
            AssetDatabase.SaveAssets();
        }
    }

    private static string NameForTile(string tilesetName, int x, int y) {
        return tilesetName + "[" + x + "][" + y + "]";
    }
}
