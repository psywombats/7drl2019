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
        { 0, "West" },
        { 1, "South" },
        { 2, "East" },
        { 3, "North" },
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

    // in the postprocessor so that hopefully we can create animations from processed textures by now
    private void OnPostprocessTexture(Texture2D texture) {
        string path = assetPath;
        char[] splitters = { '/' };
        string[] split = assetPath.Split(splitters);
        string name = split[split.Length - 1];
        name = name.Substring(0, name.IndexOf('.'));

        if (path.Contains("Charas")) {
            AssetDatabase.CreateFolder("Assets/Resources/Animations/Charas/Facings", name);

            for (int i = 0; i < 4; i += 1) {
                AnimationClip anim = new AnimationClip();
                //AnimationUtility

                AssetDatabase.CreateAsset(anim, "Assets/Resources/Animations/Charas/Facings/" + name + "/" + name + FacingNames[i] + ".anim");
            }

            AnimatorOverrideController controller = new AnimatorOverrideController();
            AssetDatabase.CreateAsset(controller, "Assets/Resources/Animations/Charas/Instances/" + name + ".overrideController");
        }
    }
}