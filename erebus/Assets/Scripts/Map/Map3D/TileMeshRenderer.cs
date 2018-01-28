using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tiled2Unity;

[RequireComponent(typeof(MeshRenderer))]
public class TileMeshRenderer : MonoBehaviour {

    public void AssignTileId(TiledMap map, Tileset tileset, int tileId) {
        Material textureAtlas = AssetDatabase.LoadAssetAtPath<Material>("Assets/Tiled2Unity/Materials/" + tileset.name + ".mat");
        GetComponent<MeshRenderer>().material = textureAtlas;

        string meshPath = "Assets/Resources/Meshes/Map3D/" + tileset.name + tileId.ToString() + ".asset";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(meshPath);
        Mesh tileMesh;
        if (assets.Length == 0) {
            tileMesh = Mesh.Instantiate<Mesh>(GetComponent<MeshFilter>().sharedMesh);
            tileMesh.name = tileId.ToString();

            int tilesetRows = textureAtlas.mainTexture.width / map.TileWidth;
            int tilesetCols = textureAtlas.mainTexture.height / map.TileHeight;
            int row = tileId % tilesetRows;
            int col = (tileId - row) / tilesetRows;
            col = tilesetCols - col - 1;
            
            List<Vector2> uvs = new List<Vector2>();
            uvs.Add(new Vector2((float)(row + 0) / (float)tilesetRows, (float)(col + 0) / (float)tilesetCols));
            uvs.Add(new Vector2((float)(row + 1) / (float)tilesetRows, (float)(col + 1) / (float)tilesetCols));
            uvs.Add(new Vector2((float)(row + 1) / (float)tilesetRows, (float)(col + 0) / (float)tilesetCols));
            uvs.Add(new Vector2((float)(row + 0) / (float)tilesetRows, (float)(col + 1) / (float)tilesetCols));
            tileMesh.SetUVs(0, uvs);

            AssetDatabase.CreateAsset(tileMesh, meshPath);
            AssetDatabase.SaveAssets();
        } else {
            tileMesh = (Mesh)assets[0];
        }
        GetComponent<MeshFilter>().mesh = tileMesh;
    }
}
