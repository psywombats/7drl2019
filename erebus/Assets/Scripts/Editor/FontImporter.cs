using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FontImporter : AssetPostprocessor {

    private const int FontSize = 8;

    public void OnPreprocessTexture() {
        if (!assetPath.ToLower().Contains("bitmapfont")) {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 1;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
    }

    public void OnPostprocessTexture(Texture2D texture) {
        if (!assetPath.ToLower().Contains("bitmapfont")) {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        string assetName = NameFromPath(assetPath);
        string assetDir = assetPath.Substring(0, assetPath.LastIndexOf('/'));
        string fontPath = assetDir + "/" + assetName + ".fontsettings";
        string materialPath = assetDir + "/" + assetName + ".mat";

        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null) {
            font = new Font();
            AssetDatabase.CreateAsset(font, fontPath);
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) {
            material = new Material(Shader.Find("Unlit/Transparent"));
            AssetDatabase.CreateAsset(material, materialPath);
        }
        font.material = material;

        int perLine = (texture.width / FontSize);
        int lineCount = (texture.height / FontSize);
        int characterCount = perLine * lineCount;
        CharacterInfo[] characterInfo = new CharacterInfo[characterCount];
        for (int y = 0; y < lineCount; y += 1) {
            for (int x = 0; x < perLine; x += 1) {
                int index = y * perLine + x;
                characterInfo[index].index = index;
                characterInfo[index].glyphWidth = FontSize;
                characterInfo[index].glyphHeight = FontSize;
                characterInfo[index].uvTopLeft =       new Vector2(1.0f / (float)perLine * (float)(x + 0), 1.0f - (1.0f / (float)lineCount * (float)(y + 0)));
                characterInfo[index].uvTopRight =      new Vector2(1.0f / (float)perLine * (float)(x + 1), 1.0f - (1.0f / (float)lineCount * (float)(y + 0)));
                characterInfo[index].uvBottomRight =   new Vector2(1.0f / (float)perLine * (float)(x + 1), 1.0f - (1.0f / (float)lineCount * (float)(y + 1)));
                characterInfo[index].uvBottomLeft =    new Vector2(1.0f / (float)perLine * (float)(x + 0), 1.0f - (1.0f / (float)lineCount * (float)(y + 1)));
                characterInfo[index].minX = 0;
                characterInfo[index].maxX = FontSize;
                characterInfo[index].minY = -FontSize;
                characterInfo[index].maxY = 0;
                characterInfo[index].advance = FontSize;
            }
        }
        font.characterInfo = characterInfo;

        AssetDatabase.SaveAssets();

        SerializedObject so = new SerializedObject(font);
        so.Update();
        so.FindProperty("m_FontSize").floatValue = FontSize;
        so.FindProperty("m_LineSpacing").floatValue = FontSize;
        so.ApplyModifiedProperties();
        so.SetIsDifferentCacheDirty();

        AssetDatabase.SaveAssets();

        ReloadFont(fontPath);
    }

    // Reeally hacky stuff from https://github.com/litefeel/Unity-BitmapFontImporter/blob/master/Assets/BitmapFontImporter/Editor/BFImporter.cs
    // no idea what the hell is up with this one but Unity 5+ only displays random glpyhs otherwise
    private static void ReloadFont(string fontPath) {
        var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        AssetDatabase.ExportPackage(fontPath, tmpPath);
        AssetDatabase.DeleteAsset(fontPath);

        var startTime = DateTime.Now;
        EditorApplication.CallbackFunction func = null;
        func = () => {
            TimeSpan dalt = DateTime.Now - startTime;
            if (dalt.TotalSeconds >= 0.1) {
                EditorApplication.update -= func;
                AssetDatabase.ImportPackage(tmpPath, false);
                File.Delete(tmpPath);
            }
        };

        EditorApplication.update += func;
    }

    private static string NameFromPath(string path) {
        char[] splitters = { '/' };
        string[] split = path.Split(splitters);
        string name = split[split.Length - 1];
        name = name.Substring(0, name.IndexOf('.'));
        return name;
    }
}
