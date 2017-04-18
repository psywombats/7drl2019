using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

internal sealed class SpriteImporter : AssetPostprocessor {

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

    public void OnPreprocessTexture() {
        string path = assetPath;
        string name = NameFromPath(path);

        if (path.Contains("Sprites") || path.Contains("UI")) {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 1;
            importer.textureType = TextureImporterType.Sprite;
            if (path.Contains("Charas")) {
                IntVector2 textureSize = GetPreprocessedImageSize();
                int stepCount = textureSize.x == 32 ? 2 : 3;
                int charaWidth = textureSize.x / stepCount;
                int charaHeight = textureSize.y / 4;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePivot = new Vector2(charaWidth / 2, Map.TileHeightPx / 2);
                importer.spritesheet = new SpriteMetaData[stepCount * 4];
                List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
                for (int y = 0; y < 4; y += 1) {
                    for (int x = 0; x < stepCount; x += 1) {
                        SpriteMetaData data = importer.spritesheet[stepCount * y + x];
                        data.rect = new Rect(x * charaWidth, y * charaHeight, charaWidth, charaHeight);
                        data.alignment = (int)SpriteAlignment.Custom;
                        data.border = new Vector4(0, 0, 0, 0);
                        data.name = name + FacingNames[y] + StepNames[x];
                        data.pivot = new Vector2(((charaWidth - Map.TileWidthPx) / 2.0f) / (float)charaWidth, 0.0f);
                        spritesheet.Add(data);
                    }
                }
                importer.spritesheet = spritesheet.ToArray();
            } else {
                importer.spriteImportMode = SpriteImportMode.Single;
            }
        }
    }

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string assetPath in importedAssets) {
            if (assetPath.Contains("Sprites/Charas")) {
                CreateAnimations(assetPath);
            }
        }
    }

    // in the postprocessor so that hopefully we can create animations from processed textures by now
    public static void CreateAnimations(string assetPath) {
        string path = assetPath;
        string name = NameFromPath(path);

        if (path.Contains("Charas")) {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Animations/Charas/Facings/" + name)) {
                AssetDatabase.CreateFolder("Assets/Resources/Animations/Charas/Facings", name);
            }

            // Some settings
            EditorCurveBinding binding = new EditorCurveBinding();
            binding.path = "";
            binding.propertyName = "m_Sprite";
            binding.type = typeof(SpriteRenderer);
            AnimationClipSettings info = new AnimationClipSettings();
            info.loopTime = true;

            Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            List<Sprite> sprites = new List<Sprite>();
            foreach (Object spriteObject in spriteObjects) {
                sprites.Add((Sprite)spriteObject);
            }
            int stepCount = sprites.Count / 4;
            for (int i = 0; i < 4; i += 1) {

                // indices seem mangled here, not sure why
                int off = 0;
               
                switch (i) {
                    case 0: off = 3; break;
                    case 1: off = 2; break;
                    case 2: off = 0; break;
                    case 3: off = 1; break;
                }

                // first up - walking animation
                AnimationClip anim = new AnimationClip();
                AnimationUtility.SetAnimationClipSettings(anim, info);
                
                List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
                if (stepCount > 2) {
                    keyframes.Add(CreateKeyframe(0.00f, sprites[off * stepCount + 1]));
                    keyframes.Add(CreateKeyframe(0.25f, sprites[off * stepCount + 0]));
                    keyframes.Add(CreateKeyframe(0.50f, sprites[off * stepCount + 2]));
                    keyframes.Add(CreateKeyframe(0.75f, sprites[off * stepCount + 0]));
                    keyframes.Add(CreateKeyframe(1.00f, sprites[off * stepCount + 1]));
                } else {
                    keyframes.Add(CreateKeyframe(0.00f, sprites[off * stepCount + 1]));
                    keyframes.Add(CreateKeyframe(0.50f, sprites[off * stepCount + 0]));
                    keyframes.Add(CreateKeyframe(1.00f, sprites[off * stepCount + 1]));
                }

                AnimationUtility.SetObjectReferenceCurve(anim, binding, keyframes.ToArray());
                string facingPath = "Assets/Resources/Animations/Charas/Facings/" + name + "/" + name + FacingNames[i] + ".anim";
                AssetDatabase.DeleteAsset(facingPath);
                AssetDatabase.CreateAsset(anim, facingPath);

                // next up - idle animation
                if (stepCount > 2) {
                    anim = new AnimationClip();
                    AnimationUtility.SetAnimationClipSettings(anim, info);

                    keyframes = new List<ObjectReferenceKeyframe>();
                    keyframes.Add(CreateKeyframe(0.00f, sprites[off * stepCount]));
                    AnimationUtility.SetObjectReferenceCurve(anim, binding, keyframes.ToArray());
                    facingPath = "Assets/Resources/Animations/Charas/Facings/" + name + "/" + name + FacingNames[i] + "Idle.anim";
                    AssetDatabase.DeleteAsset(facingPath);
                    AssetDatabase.CreateAsset(anim, facingPath);
                }
            }

            AnimatorOverrideController controller = new AnimatorOverrideController();
            controller.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Resources/Animations/Charas/CharaController.controller");
            List<AnimationClipPair> clips = new List<AnimationClipPair>();
            string facingsDir = "Assets/Resources/Animations/Charas/Facings/";
            for (int i = 0; i < 4; i += 1) {
                AnimationClipPair clip = new AnimationClipPair();
                clip.originalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(facingsDir + "Placeholder/Placeholder" + FacingNames[i] + ".anim");
                clip.overrideClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(facingsDir + name + "/" + name + FacingNames[i] + ".anim");
                clips.Add(clip);
            }
            for (int i = 0; i < 4; i += 1) {
                AnimationClipPair clip = new AnimationClipPair();
                clip.originalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(facingsDir + "Placeholder/Placeholder" + FacingNames[i] + "Idle.anim");
                if (stepCount > 2) {
                    clip.overrideClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(facingsDir + name + "/" + name + FacingNames[i] + "Idle.anim");
                } else {
                    clip.overrideClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(facingsDir + name + "/" + name + FacingNames[i] + ".anim");
                }
                clips.Add(clip);
            }
            controller.clips = clips.ToArray();
            string overridePath = "Assets/Resources/Animations/Charas/Instances/" + name + ".overrideController";
            AssetDatabase.DeleteAsset(overridePath);
            AssetDatabase.CreateAsset(controller, overridePath);
        }
    }

    private static ObjectReferenceKeyframe CreateKeyframe(float time, Sprite sprite) {
        ObjectReferenceKeyframe keyframe = new ObjectReferenceKeyframe();
        keyframe.time = time;
        keyframe.value = sprite;
        return keyframe;
    }

    private static string NameFromPath(string path) {
        char[] splitters = { '/' };
        string[] split = path.Split(splitters);
        string name = split[split.Length - 1];
        name = name.Substring(0, name.IndexOf('.'));
        return name;
    }

    private IntVector2 GetPreprocessedImageSize() {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null) {
            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(importer, args);
            return new IntVector2((int)args[0], (int)args[1]);
        }

        return new IntVector2(0, 0);
    }

}
