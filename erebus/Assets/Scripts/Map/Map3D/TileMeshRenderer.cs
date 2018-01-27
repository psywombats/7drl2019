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

        int tilesetRows = textureAtlas.mainTexture.width / map.TileWidth;
        int tilesetCols = textureAtlas.mainTexture.height / map.TileHeight;
        int col = tileId % tilesetRows;
        int row = (tileId - col) / tilesetRows;
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(new Vector2((float)(row + 0) / (float)tilesetRows, (float)(col + 0) / (float)tilesetCols));
        uvs.Add(new Vector2((float)(row + 1) / (float)tilesetRows, (float)(col + 1) / (float)tilesetCols));
        uvs.Add(new Vector2((float)(row + 1) / (float)tilesetRows, (float)(col + 0) / (float)tilesetCols));
        uvs.Add(new Vector2((float)(row + 0) / (float)tilesetRows, (float)(col + 0) / (float)tilesetCols));
        mesh.SetUVs(0, uvs);
    }
}
