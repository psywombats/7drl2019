using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal sealed class SpriteImporter : AssetPostprocessor {

    public void OnPreprocessTexture() {
        string path = assetPath;
        string name = EditorUtils.NameFromPath(path);

        if (name.Contains("Placeholder")) {
            return;
        }

        if (path.Contains("Sprites") || path.Contains("UI") || path.Contains("tilesets")) {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.textureType = TextureImporterType.Sprite;
            Vector2Int textureSize = EditorUtils.GetPreprocessedImageSize(importer);
            if (path.Contains("Charas")) {
                int edgeSizeX = textureSize.x == 72 ? 24 : 32;
                int edgeSizeY = 32;
                int cols = textureSize.x / edgeSizeX;
                int rows = textureSize.y / edgeSizeY;
                importer.spritePixelsPerUnit = Map.TileSizePx;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritesheet = new SpriteMetaData[rows * cols];
                List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
                for (int y = 0; y < rows; y += 1) {
                    for (int x = 0; x < cols; x += 1) {
                        SpriteMetaData data = importer.spritesheet[y * cols + x];
                        data.rect = new Rect(x * edgeSizeX, (cols - y) * edgeSizeY, edgeSizeX, edgeSizeY);
                        data.alignment = (int)SpriteAlignment.Custom;
                        data.border = new Vector4(0, 0, 0, 0);
                        data.name = CharaEvent.NameForFrame(name, x, y);
                        data.pivot = new Vector2(0.5f, 0.0f);
                        spritesheet.Add(data);
                    }
                }
                importer.spritesheet = spritesheet.ToArray();
            }
        }
    }
}
