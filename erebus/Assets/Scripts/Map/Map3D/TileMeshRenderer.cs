using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;

[RequireComponent(typeof(MeshRenderer))]
public class TileMeshRenderer : MonoBehaviour {

    public void AssignTileId(TiledMap map, Tileset tileset, int tileId) {
        Material textureAtlas = Resources.Load<Material>("Materials/" + tileset.name);
        GetComponent<MeshRenderer>().material = textureAtlas;

        int tilesetRows = textureAtlas.mainTexture.width / map.TileWidth;
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        List<Vector2> uvs = new List<Vector2>();
        mesh.GetUVs(tilesetRows, uvs);
        mesh.SetUVs(0, uvs);
    }
}
