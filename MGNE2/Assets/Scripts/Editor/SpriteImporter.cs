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
        string name = NameFromPath(path);

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
                        data.name = name + FacingNames[y] + StepNames[x];
                        data.pivot = new Vector2(4.0f / (float)CharaWidth, 0.0f);
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
        string name = NameFromPath(path);

        if (path.Contains("Charas")) {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Charas/Facings/" + name)) {
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Charas/Facings", name);
            }
            
            Sprite[] sprites = Resources.LoadAll<Sprite>(texture.name);
            for (int i = 0; i < 4; i += 1) {
                AnimationClip anim = new AnimationClip();
                AnimationClipSettings info = new AnimationClipSettings();
                info.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(anim, info);
                
                EditorCurveBinding binding = new EditorCurveBinding();
                binding.path = "";
                binding.propertyName = "m_Sprite";
                binding.type = typeof(SpriteRenderer);
                
                List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
                int off = 0;
                // indices seem mangled here, not sure why
                switch (i) {
                    case 0:     off = 3;    break;
                    case 1:     off = 2;    break;
                    case 2:     off = 0;    break;
                    case 3:     off = 1;    break;
                }
                keyframes.Add(CreateKeyframe(0.00f, sprites[off * 3 + 1]));
                keyframes.Add(CreateKeyframe(0.25f, sprites[off * 3 + 0]));
                keyframes.Add(CreateKeyframe(0.50f, sprites[off * 3 + 2]));
                keyframes.Add(CreateKeyframe(0.75f, sprites[off * 3 + 0]));
                keyframes.Add(CreateKeyframe(1.00f, sprites[off * 3 + 1]));

                AnimationUtility.SetObjectReferenceCurve(anim, binding, keyframes.ToArray());
                string facingPath = "Assets/Resources/Animations/Charas/Facings/" + name + "/" + name + FacingNames[i] + ".anim";
                AssetDatabase.DeleteAsset(facingPath);
                AssetDatabase.CreateAsset(anim, facingPath);
            }

            AnimatorOverrideController controller = new AnimatorOverrideController();
            controller.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Resources/Animations/Charas/CharaController.controller");
            List<AnimationClipPair> clips = new List<AnimationClipPair>();
            for (int i = 0; i < 4; i += 1) {
                AnimationClipPair clip = new AnimationClipPair();
                clip.originalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/Charas/Facings/Placeholder/Placeholder" + FacingNames[i] + ".anim");
                clip.overrideClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/Charas/Facings/" + name + "/" + name + FacingNames[i] + ".anim");
                clips.Add(clip);
            }
            controller.clips = clips.ToArray();
            string overridePath = "Assets/Resources/Animations/Charas/Instances/" + name + ".overrideController";
            AssetDatabase.DeleteAsset(overridePath);
            AssetDatabase.CreateAsset(controller, overridePath);
        }
    }

    private ObjectReferenceKeyframe CreateKeyframe(float time, Sprite sprite) {
        ObjectReferenceKeyframe keyframe = new ObjectReferenceKeyframe();
        keyframe.time = time;
        keyframe.value = sprite;
        return keyframe;
    }

    private string NameFromPath(string path) {
        char[] splitters = { '/' };
        string[] split = assetPath.Split(splitters);
        string name = split[split.Length - 1];
        name = name.Substring(0, name.IndexOf('.'));
        return name;
    }
}
