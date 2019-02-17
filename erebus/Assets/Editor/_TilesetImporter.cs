﻿using UnityEngine;
using UnityEditor;

public class MeshEditCustomImpoter : AssetPostprocessor
{
    // This sets up the tilesets we create to be read by the Mesh Edit tools. It also converts them to the correct pixel-art import settings
    void OnPreprocessTexture()
    {
        string cutOffString = "Resources/";
        string assetName = assetPath.Remove(0, assetPath.LastIndexOf(cutOffString) + cutOffString.Length);
        assetName = assetName.Remove(assetName.Length - 4);

        TextureImporter texImporter = (TextureImporter)assetImporter;

        if (assetPath.Contains("EditorResources/Tilesets/"))
        {
            Debug.Log("Tileset imported.");
            texImporter.isReadable = true;
            texImporter.textureType = TextureImporterType.Sprite;
            texImporter.spritePixelsPerUnit = 32;
            texImporter.mipmapEnabled = false;
            texImporter.spritePivot = new Vector2(0.5f, 0.5f);
            texImporter.textureCompression = TextureImporterCompression.Uncompressed;
            texImporter.wrapMode = TextureWrapMode.Clamp;
            texImporter.filterMode = FilterMode.Point;
            texImporter.alphaIsTransparency = true;
            texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            texImporter.spriteImportMode = SpriteImportMode.Multiple;
            texImporter.npotScale = TextureImporterNPOTScale.None;
        }
        else if (assetPath.Contains("Tilesets/Constructed/"))
        {
            Debug.Log("Texture page imported.");
            texImporter.textureType = TextureImporterType.Sprite;
            texImporter.spritePixelsPerUnit = 32;
            texImporter.mipmapEnabled = false;
            texImporter.spritePivot = new Vector2(0.5f, 0.5f);
            texImporter.textureCompression = TextureImporterCompression.Uncompressed;
            texImporter.wrapMode = TextureWrapMode.Clamp;
            texImporter.filterMode = FilterMode.Point;
            texImporter.alphaIsTransparency = true;
            texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            texImporter.spriteImportMode = SpriteImportMode.Multiple;
            texImporter.npotScale = TextureImporterNPOTScale.None;
        }
    }
}