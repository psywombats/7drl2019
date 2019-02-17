using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class TilesetManager : EditorWindow
{
    bool groupEnabled;

    List<Tileset> tilesetsAvailable;
    List<string> tilesetTexturesAvailable;
    int selectedTileset;
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Tilesets")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(TilesetManager));
    }
    bool isUpToDate = true;
    Texture2D newTexture = null;
    Texture2D newAnimation = null;
    List<Texture2D> textures;
    List<Texture2D> animations;
    string tilesetName;
    int tileWidth;
    int tileHeight;
    int tileOutline;

    Vector2 scrollPos = Vector2.zero;
    void Update()
    {
        if (textures == null || animations == null)
        {
            int oldSelected = selectedTileset;
            selectedTileset = -1;
            changeActiveTileset(oldSelected);
        }

        if (textures != null && textures.Count > 0)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i] == null)
                {
                    textures.RemoveAt(i);
                    i--;
                }
            }
        }
        if (animations != null && animations.Count > 0)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                if (animations[i] == null)
                {
                    animations.RemoveAt(i);
                    i--;
                }
            }
        }
        if (newTexture != null && focusedWindow == this)
        {
            if (isTextureReadable(newTexture))
            {
                isUpToDate = false;
                textures.Add(newTexture);
            }
            else
            {
                Debug.Log("Error! The texture chosen was un-readable. Textures added to a tileset must be made readable in the editor.");
            }
            newTexture = null;
        }

        if (newAnimation != null && focusedWindow == this)
        {
            if (isTextureReadable(newAnimation))
            {
                isUpToDate = false;
                animations.Add(newAnimation);
            }
            else
            {
                Debug.Log("Error! The animation chosen was un-readable. Textures added to a tileset must be made readable in the editor.");
            }
            newAnimation = null;
        }

    }

    void OnGUI()
    {
        // GUILayout.Label("Base Settings", EditorStyles.boldLabel);




        // GUILayout.Label(EditorWindow.focusedWindow.ToString());


        if (tilesetsAvailable == null)
        {
            loadTilesets();
        }
        if (tilesetTexturesAvailable == null)
        {
            loadTilesets();
        }
        // Tileset selection (Drop down)
        int nextTileset = selectedTileset;
        nextTileset = EditorGUILayout.Popup(selectedTileset, tilesetTexturesAvailable.ToArray(), GUILayout.Width(100));
        if (nextTileset != selectedTileset)
        {
            changeActiveTileset(nextTileset);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ New Tileset", GUILayout.Width(100)))
        {
            Tileset tileset = new Tileset();
            string fileName = "New Tileset";
            int num = 0;
            while (tilesetTexturesAvailable.Contains(fileName))
            {
                num++;
                fileName = "New Tileset (" + num.ToString() + ")";

            }
            tileset.tilesetName = fileName;
            tilesetName = fileName;

            tileWidth = 64;
            tileHeight = 64;

            tilesetsAvailable.Add(tileset);
            tilesetTexturesAvailable.Add(fileName);

            changeActiveTileset(tilesetsAvailable.Count - 1);
        }
        if (tilesetsAvailable.Count > 0 && selectedTileset >= 0 && selectedTileset < tilesetsAvailable.Count)
        { 
            if (GUILayout.Button("- Delete Tileset", GUILayout.Width(100)))
            {
                // Delete the files from the directories
                deleteTilesetFiles(selectedTileset);
                AssetDatabase.Refresh();
                // Remove from the list
                tilesetsAvailable.RemoveAt(selectedTileset);
                tilesetTexturesAvailable.RemoveAt(selectedTileset);
                nextTileset = selectedTileset - 1;
                if (nextTileset < 0) { nextTileset = 0; }

                // Set up values so that the deleted tileset is not referenced, and a tileset change is forced
                isUpToDate = true;
                selectedTileset = -1;
                changeActiveTileset(nextTileset);
            }
        }
        GUILayout.EndHorizontal();

        if (tilesetsAvailable.Count > 0 && selectedTileset >= 0 && selectedTileset < tilesetsAvailable.Count)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("File Name ", GUILayout.Width(80));
            string oldTilesetName = tilesetName;
            tilesetName = EditorGUILayout.TextField(tilesetName);
            if (oldTilesetName != tilesetName)
            {
                isUpToDate = false;
            }

            GUILayout.FlexibleSpace();

            //GUI.enabled = (!isUpToDate);
            string buttonName = "Apply Changes";
            if (isUpToDate) { buttonName = "Re-Apply Changes"; }
            if (GUILayout.Button(buttonName)) 
            {
                applyChanges();
                Selection.activeGameObject = null;
            }
            //GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            int newTileWidth = tileWidth;
            EditorGUILayout.LabelField("Tile Width ", GUILayout.Width(80));
            newTileWidth = EditorGUILayout.IntField(tileWidth, GUILayout.Width(70));
            if (newTileWidth != tileWidth && newTileWidth > 0)
            {
                tileWidth = newTileWidth;
                isUpToDate = false;
            }

            int newTileHeight = tileHeight;
            EditorGUILayout.LabelField("Tile Height ", GUILayout.Width(80));
            newTileHeight = EditorGUILayout.IntField(tileHeight, GUILayout.Width(70));
            if (newTileHeight != tileHeight && newTileHeight > 0)
            {
                tileHeight = newTileHeight;
                isUpToDate = false;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            int newTileOutline = tileOutline;
            EditorGUILayout.LabelField("Tile Spacing ", GUILayout.Width(80));
            newTileOutline = EditorGUILayout.IntField(tileOutline, GUILayout.Width(70));
            if (newTileOutline != tileOutline && newTileOutline >= 1)
            {
                tileOutline = newTileOutline;
                isUpToDate = false;
            }

            EditorGUILayout.EndHorizontal();
            scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos);
            if (tilesetTexturesAvailable != null && tilesetTexturesAvailable.Count > 0)
            {
                EditorGUILayout.LabelField("Textures", EditorStyles.boldLabel);

                for (int i = 0; i < textures.Count; i++)
                {
                    Texture2D oldT2D = textures[i];
                    textures[i] = (Texture2D)EditorGUILayout.ObjectField(textures[i], typeof(Texture2D), false, GUILayout.Width(240), GUILayout.Height(16));
                    
                    if (oldT2D != textures[i])
                    {
                        isUpToDate = false;
                    }
                }
                newTexture = (Texture2D)EditorGUILayout.ObjectField(newTexture, typeof(Texture2D), false, GUILayout.Width(240), GUILayout.Height(16));

                EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

                for (int i = 0; i < animations.Count; i++)
                {
                    Texture2D oldT2D = animations[i];
                    animations[i] = (Texture2D)EditorGUILayout.ObjectField(animations[i], typeof(Texture2D), false, GUILayout.Width(240), GUILayout.Height(16));

                    if (oldT2D != animations[i])
                    {
                        isUpToDate = false;
                    }
                }

                newAnimation = (Texture2D)EditorGUILayout.ObjectField(newAnimation, typeof(Texture2D), false, GUILayout.Width(240), GUILayout.Height(16));
                
            }

            GUILayout.Space(32);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (tilesetsAvailable[selectedTileset].textureAssets != null && tilesetsAvailable[selectedTileset].textureAssets.Count > 0 &&
                tilesetsAvailable[selectedTileset].animationAssets != null && tilesetsAvailable[selectedTileset].animationAssets.Count > 0)
            {
                EditorGUILayout.LabelField("Page size: " + tilesetsAvailable[selectedTileset].texturePage.width + "x" + tilesetsAvailable[selectedTileset].texturePage.height, GUILayout.Width(120));
            }

            EditorGUILayout.EndHorizontal();
        }

        if (Event.current.type == EventType.Layout)
        {

        }
    }

    private void changeActiveTileset(int index)
    {
        if (index != selectedTileset)
        {
            if (tilesetsAvailable != null && tilesetsAvailable.Count > 0)
            {
                selectedTileset = index;
                isUpToDate = true;
                tilesetsAvailable[selectedTileset].loadTexturesFromAssets();
                tilesetName = tilesetsAvailable[selectedTileset].tilesetName;
                tileWidth = tilesetsAvailable[selectedTileset].tileWidth;
                tileHeight = tilesetsAvailable[selectedTileset].tileHeight;
                tileOutline = tilesetsAvailable[selectedTileset].tileOutline;
                textures = new List<Texture2D>();
                for (int i = 0; i < tilesetsAvailable[selectedTileset].textureAssets.Count; i++)
                {
                    textures.Add(tilesetsAvailable[selectedTileset].textures[i]);
                }
                animations = new List<Texture2D>();
                for (int i = 0; i < tilesetsAvailable[selectedTileset].animations.Count; i++)
                {
                    animations.Add(tilesetsAvailable[selectedTileset].animations[i]);
                }
            }
        }
    }
    
    private void applyChanges()
    {
        Debug.Log("Updating tileset");
        deleteTilesetFiles(selectedTileset);

        tilesetsAvailable[selectedTileset].reConstructTileset(tilesetName, textures, animations, tileWidth, tileHeight, tileOutline);
        tilesetsAvailable[selectedTileset].packTextures();
        tilesetsAvailable[selectedTileset].saveTexturePageToFile(tilesetName);
        saveTileset(selectedTileset);
        AssetDatabase.Refresh();
        loadTilesets();
        isUpToDate = true;

        MeshEdit[] meshes = GameObject.FindObjectsOfType<MeshEdit>();

        for (int i = 0; i < meshes.Length; i++)
        {
            bool refresh = false;

            for (int ii = 0; ii < meshes[i].uvMaps.Count; ii++)
            {
                if (meshes[i].uvMaps[ii].name == tilesetName)
                {
                    refresh = true;

                    meshes[i].uvMaps[ii].resizeUVSpace(
                        tilesetsAvailable[selectedTileset].texturePage.width,
                        tilesetsAvailable[selectedTileset].texturePage.height,
                        tilesetsAvailable[selectedTileset].tileWidth,
                        tilesetsAvailable[selectedTileset].tileHeight,
                        tilesetsAvailable[selectedTileset].tileOutline);

                }
            }

            if (refresh)
            {
                meshes[i].pushUVData();
                meshes[i].isTilesetRefreshRequired = true;
            }
        }
        
    }

    private void deleteTilesetFiles(int selectedTileset)
    {

        // Delete original XML file
        string path = Application.dataPath + "/Editor/EditorResources/Tilesets/" + tilesetsAvailable[selectedTileset].tilesetName + ".xml";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        // Delete the editor tileset
        path = Application.dataPath + "/Editor/EditorResources/Tilesets/" + tilesetsAvailable[selectedTileset].tilesetName + ".png";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        // Delete the constructed texture page
        path = Application.dataPath + "/Resources/Tilesets/Constructed/" + tilesetsAvailable[selectedTileset].tilesetName + ".png";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private void saveTileset(int tilesetIndex)
    {
        SerializeObject(tilesetsAvailable[tilesetIndex], Application.dataPath + "/Editor/EditorResources/Tilesets/" + tilesetsAvailable[tilesetIndex].tilesetName);
    }

    private void loadTilesets()
    {


        tilesetTexturesAvailable = new List<string>();

        tilesetsAvailable = new List<Tileset>();
        
        DirectoryInfo d = new DirectoryInfo(Application.dataPath + "/Editor/EditorResources/Tilesets");
        FileInfo[] files = d.GetFiles();

        foreach (FileInfo xmlFile in files)
        {
            if (xmlFile.Extension.Contains("xml"))
            {
                Tileset tileset = DeSerializeObject<Tileset>(xmlFile.FullName);
                tileset.loadTexturesFromAssets();
                //tileset.packTextures();

                // If either file has been manually renamed then make sure no asset is loaded, since it would cause a FileNotFound error when loading the texture page.
                
                tilesetsAvailable.Add(tileset);
                tilesetTexturesAvailable.Add(tileset.tilesetName);
                
            }
        }
    }

    private void createTileset(string name)
    {

    }

    public void SerializeObject<T>(T serializableObject, string fileName)
    {
        if (serializableObject == null) { return; }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName + ".xml");
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            throw (ex);
        }
    }

    public bool isTextureReadable(Texture2D texture)
    {
        
        string texturePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        if (textureImporter.isReadable)
        {
            return true;
        }
        textureImporter.isReadable = true;
        AssetDatabase.ImportAsset(texturePath);
        
        AssetDatabase.Refresh();
        return true;
        
        // Instead of checking, we can just set the texture to be readable, read the texture and then set it back to it's original state.
    }

    /// <summary>
    /// Deserializes an xml file into an object list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T DeSerializeObject<T>(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) { return default(T); }

        T objectOut = default(T);

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;

            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
        }
        catch (Exception ex)
        {
            throw (ex);
        }

        return objectOut;
    }
}