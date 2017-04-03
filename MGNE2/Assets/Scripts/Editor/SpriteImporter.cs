using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal sealed class SpriteImporter : AssetPostprocessor {

    private static readonly int CharaWidth = 24;
    private static readonly int CharaHeight = 32;

    private static readonly Dictionary<int, string> StepNames = new Dictionary<int, string> {
        { 0, "Left" },
        { 1, "Center" },
        { 2, "Right" },
    };
    private static readonly Dictionary<int, string> FacingNames = new Dictionary<int, string> {
        { 0, "North" },
        { 1, "East" },
        { 2, "South" },
        { 3, "West" },
    };

    private void OnPreprocessTexture() {
        string path = assetPath;
        
        if (path.Contains("Sprites")) {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 1;
            importer.textureType = TextureImporterType.Sprite;
            if (path.Contains("Charas")) {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePivot = new Vector2(CharaWidth / 2, CharaHeight / 4);
                importer.spritesheet = new SpriteMetaData[12];
                List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
                for (int y = 0; y < 4; y += 1) {
                    for (int x = 0; x < 3; x += 1) {
                        SpriteMetaData data = importer.spritesheet[3 * y + x];
                        data.rect = new Rect(x * CharaWidth, y * CharaHeight, CharaWidth, CharaHeight);
                        data.alignment = (int)SpriteAlignment.Custom;
                        data.border = new Vector4(0, 0, 0, 0);
                        data.name = importer.name + FacingNames[y] + StepNames[x];
                        data.pivot = new Vector2(CharaWidth / 2, CharaHeight / 4);
                        spritesheet.Add(data);
                    }
                }
                importer.spritesheet = spritesheet.ToArray();
            } else {
                importer.spriteImportMode = SpriteImportMode.Single;
            }
        }
    }
}