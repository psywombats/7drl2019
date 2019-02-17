using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
/// <summary>
/// The tileset used by the mesh tiling interface
/// </summary>
[Serializable]
public class Tileset : System.Object
{
    [Serializable]
    private class UVAnimation : System.Object
    {
        [SerializeField, HideInInspector]
        public int startTileX;
        [SerializeField, HideInInspector]
        public int startTileY;
        [SerializeField, HideInInspector]
        public int imageSpeed;
        [SerializeField, HideInInspector]
        public int imageCount;
    }
    [SerializeField, HideInInspector]
    List<UVAnimation> uvAnimations;

    [SerializeField, HideInInspector]
    public List<Vector2> tilesetMappedPoints;
    [SerializeField, HideInInspector]
    public List<Vector2> editorMappedPoints;
    [SerializeField, HideInInspector]
    public int[] animationLength;

    [SerializeField, HideInInspector]
    public string tilesetName;

    [SerializeField, HideInInspector]
    public int tileWidth, tileHeight;

    [SerializeField, HideInInspector]
    public int tileOutline;

    [SerializeField, HideInInspector]
    public int pageWidth, pageHeight;
    [SerializeField, HideInInspector]
    public int editorTilesetWidth, editorTilesetHeight;
    [SerializeField, HideInInspector]
    public int tilePadding = 0;

    [SerializeField, HideInInspector]
    public List<string> textureAssets;
    [SerializeField, HideInInspector]
    public List<string> animationAssets;
    [SerializeField, HideInInspector, XmlIgnore]
    public Texture2D texturePage;
    [SerializeField, HideInInspector, XmlIgnore]
    Texture2D textureEditorTileset;

    [SerializeField, HideInInspector, XmlIgnore]
    public List<Texture2D> textures;
    [SerializeField, HideInInspector, XmlIgnore]
    public List<Texture2D> animations;

#if UNITY_EDITOR
    public Tileset()
    {
        initialise();
    }
    
    public void reConstructTileset(string name, List<Texture2D> textures, List<Texture2D> animations, int tileWidth, int tileHeight, int tileOutline)
    {
        this.tilesetName = name;
        initialise();

        int widestTexture = 0;

        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;
        this.tileOutline = tileOutline;

        for (int i = 0; i < textures.Count; i++)
        {
            string fullName = AssetDatabase.GetAssetPath(textures[i]);
            this.textures.Add(textures[i]);
            this.textureAssets.Add(fullName);
            
            if (textures[i].width > widestTexture)
            {
                widestTexture = textures[i].width;
            }
        }
        for (int i = 0; i < animations.Count; i++)
        {
            string fullName = AssetDatabase.GetAssetPath(animations[i]);
            this.animations.Add(animations[i]);
            this.animationAssets.Add(fullName);
        }
    }

    private void initialise()
    {
        pageWidth = 8 * 64;
        pageHeight = 8 * 64;
        tileWidth = 64;
        tileHeight = 64;
        tileOutline = 1;

        textures = new List<Texture2D>();
        animations = new List<Texture2D>();

        textureAssets = new List<string>();
        animationAssets = new List<string>();
    }

    public void loadTexturesFromAssets()
    {
        textures = new List<Texture2D>();
        animations = new List<Texture2D>();
        for (int i = 0; i < textureAssets.Count; i++)
        {
            string path = textureAssets[i];
            textures.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        }
        for (int i = 0; i < animationAssets.Count; i++)
        {
            string path = animationAssets[i];
            animations.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        }
        checkEditorFoldersExist();

        texturePage = Resources.Load<Texture2D>("Tilesets/Constructed/" + tilesetName);
    }

    /// <summary>
    /// Adds a texture or animation to the tileset.
    /// Fails if the new texture doesn't fit the tileWidth/tileHeight 
    ///     consistency or is too wide for the texture page.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="isAnimation"></param>
    /// <returns></returns>
    public bool addTexture(string texturePath, bool isAnimation)
    {
        // Insert to keep textures sorted by width

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        float w = texture.width;

        if (w > pageWidth)
        {
            return false;
        }
        if (w % tileWidth != 0 || texture.height % tileHeight != 0)
        {
            return false;
        }

        if (isAnimation)
        {
            animations.Add(texture);
        }
        else
        {
            int insertionPos = 0;

            for (insertionPos = 0; insertionPos < textures.Count; insertionPos++)
            {
                if (textures[insertionPos].width < w)
                {
                    break;
                }
            }
            textures.Insert(insertionPos, texture);
        }

        packTextures();

        return true;
    }

    public void removeTexture(int index)
    {
        textures.RemoveAt(index);
        textureAssets.RemoveAt(index);
        packTextures();
        saveTexturePageToFile(tilesetName);
    }
    public void packTextures()
    {
        Debug.Log("Packing texture page for tileset: " + tilesetName);
        int packedHeight = 0;
        int widestTexture = 0;

        int fullTileWidth = tileWidth + tileOutline * 2;
        int fullTileHeight = tileHeight + tileOutline * 2;

        for (int i = 0; i < textures.Count; i++)
        {
            packedHeight += textures[i].height;
            if (textures[i].width > widestTexture)
            {
                widestTexture = textures[i].width;
            }
        }

        pageWidth = widestTexture;

        int tilesOccupied = 0;
        int tilesPerRow = pageWidth / tileWidth;
        int animationRows = 0;

        // Add the outlines into the width of the texture page
        int editorTilesetWidth = pageWidth;
        pageWidth += (tileOutline * 2 * tilesPerRow);

        for (int i = 0; i < animations.Count; i++)
        {
            tilesOccupied += (animations[i].width / tileWidth) * (animations[i].height / tileHeight);
            while (tilesOccupied > tilesPerRow)
            {
                tilesOccupied -= tilesPerRow;
                animationRows++;
            }
        }
        if (tilesOccupied > 0)
        {
            animationRows++;
        }

        pageHeight = packedHeight;

        int editorPackedHeight = packedHeight;
        if (animations.Count > 0) { editorPackedHeight += (animations.Count / tilesPerRow + 1) * tileHeight; }
        
        pageHeight += animationRows * tileHeight;

        pageHeight += (pageHeight / tileHeight) * tileOutline * 2;

        texturePage = new Texture2D(pageWidth, pageHeight, TextureFormat.RGBA32, false);
        textureEditorTileset = new Texture2D(editorTilesetWidth, editorPackedHeight, TextureFormat.RGBA32, false);
        editorMappedPoints = new List<Vector2>();
        tilesetMappedPoints = new List<Vector2>();
        
        editorTilesetHeight = editorPackedHeight;
        this.editorTilesetWidth = editorTilesetWidth;

        animationLength = new int[(editorTilesetWidth / tileWidth) * (editorTilesetHeight / tileHeight)];

        // blit all textures vertically, sorted by width
        int texY = 0;
        int texPageY = 0;
        for (int i = 0; i < textures.Count; i++)
        {
            texY += textures[i].height;
            texPageY += textures[i].height + (textures[i].height / tileHeight) * (tileOutline * 2);
            //blitPixelsToTexture(0, pageHeight - texY, textures[i], texturePage);
            blitPixelsToTexture(0, editorPackedHeight - texY, textures[i], textureEditorTileset);

            int tileCount = (textures[i].width / tileWidth) * (textures[i].height / tileWidth);

            int tilesPerRowInTexture = textures[i].width / tileWidth;

            // blit one tile at a time to ensure proper outlines
            for (int ii = 0; ii < tileCount; ii++)
            {
                int srcTileX = (ii % tilesPerRowInTexture) * (tileWidth);
                int srcTileY = (ii / tilesPerRowInTexture) * (tileHeight);

                int tileX = (ii % tilesPerRowInTexture) * fullTileWidth;
                int tileY = (ii / tilesPerRowInTexture) * fullTileHeight;

                blitPixelsToTexture(tileX, pageHeight - texPageY + tileY, srcTileX, srcTileY, tileWidth, tileHeight, textures[i], texturePage, tileOutline);
            }
        }

        // blit all animation frames as a block spritesheet
        uvAnimations = new List<UVAnimation>();
        int startX = 0;
        int startY = texPageY;
        for (int i = 0; i < animations.Count; i++)
        {
            int tileCount = (animations[i].width / tileWidth) * (animations[i].height / tileHeight);
            UVAnimation animationData = new UVAnimation();
            animationData.imageCount = tileCount;
            animationData.startTileX = startX;
            animationData.startTileY = startY;
            animationData.imageSpeed = 4;
            uvAnimations.Add(animationData);
           
            int animX = (i % tilesPerRow ) * tileWidth;
            int animY = editorPackedHeight - (texY + i / tilesPerRow) - tileHeight;
            blitPixelsToTexture(animX, animY, 0, 0, tileWidth, tileHeight, animations[i], textureEditorTileset);
            animationLength[(animX / tileWidth)+ ( animY / tileHeight) * (pageWidth / (fullTileWidth))] = tileCount;


            tilesetMappedPoints.Add(new Vector2(i % tilesPerRow, texY / tileHeight + i / tilesPerRow));
            editorMappedPoints.Add(new Vector2(startX / fullTileWidth, startY / fullTileHeight));

            // blit one tile at a time to save space
            for (int ii = 0; ii < tileCount; ii++)
            {
                blitPixelsToTexture(
                    startX, 
                    pageHeight - startY - fullTileHeight,
                    (ii * tileWidth) % animations[i].width, 
                    tileHeight * (ii / (animations[i].width / tileWidth)), 
                    tileWidth, 
                    tileHeight, 
                    animations[i], 
                    texturePage, tileOutline);
                
                startX += tileWidth + tileOutline * 2;
                if (startX >= tilesPerRow * tileWidth + tileOutline * 2)
                {
                    startX = 0;
                    startY += tileHeight + tileOutline * 2;
                }
            }
        }
        
    }

    private void blitPixelsToTexture(int x, int y, Texture2D sourceTexture, Texture2D targetTexture)
    {
        blitPixelsToTexture(x, y, 0, 0, sourceTexture.width, sourceTexture.height, sourceTexture, targetTexture);
    }
    private void blitPixelsToTexture(int destX, int destY, int srcX, int srcY, int srcWidth, int srcHeight, Texture2D sourceTexture, Texture2D targetTexture, int outline = 0)
    {
        for (int y = 0; y < srcHeight + outline * 2; y++)
        {
            for (int x = 0; x < srcWidth + outline * 2; x++)
            {

                int srcXX = x - outline;
                int srcYY = y - outline;
                if (srcXX < 0) { srcXX = 0; }
                if (srcYY < 0) { srcYY = 0; }
                if (srcXX >= srcWidth) { srcXX = srcWidth - 1; }
                if (srcYY >= srcHeight) { srcYY = srcHeight - 1; }

                Color col = sourceTexture.GetPixel(srcX + srcXX, srcY + srcYY);

                targetTexture.SetPixel(destX + x, destY + y, col);
            }
        }
    }

    public static void checkEditorFoldersExist()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Editor/EditorResources"))
        {
            AssetDatabase.CreateFolder("Assets/Editor", "EditorResources");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Editor/EditorResources/Tilesets"))
        {
            AssetDatabase.CreateFolder("Assets/Editor/EditorResources", "Tilesets");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Tilesets"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Tilesets");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Tilesets/Constructed"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Tilesets", "Constructed");
        }
    }

    public void saveTexturePageToFile(string fileName)
    {
        if (texturePage != null)
        {
            checkEditorFoldersExist();

            byte[] bytes = texturePage.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath + "/Resources/Tilesets/Constructed/" + fileName + ".png", bytes);
            
            // Importasset has to have the extension added, resources.load doesn't.
            AssetDatabase.ImportAsset("Assets/Resources/Tilesets/Constructed/" + fileName + ".png");
           

            byte[] bytesEditor = textureEditorTileset.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath + "/Editor/EditorResources/Tilesets/" + fileName + ".png", bytesEditor);
            
            AssetDatabase.ImportAsset("Assets/Editor/EditorResources/Tilesets/" + fileName + ".png");
            
            AssetDatabase.Refresh();
            
        }
    }

    public int getAnimationLength(int selectedTile)
    {
        if (animationLength == null)
        {
            packTextures();
        }
        int w = (editorTilesetWidth / tileWidth);
        int h = (editorTilesetHeight / tileHeight);
        int selectedX = selectedTile % w;
        int selectedY = (h - 1) - selectedTile / w;

        return animationLength[selectedX + selectedY * w];
    }
#endif
}
