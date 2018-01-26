using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;

[RequireComponent(typeof(MeshRenderer))]
public class TileMeshRenderer : MonoBehaviour {

    public void AssignTileId(TiledMap map, Tileset tileset, int tileId) {
        Material textureAtlas = Resources.Load<Material>("Materials/" + tileset.name);
        GetComponent<MeshRenderer>().material = textureAtlas;

        List<Vector2> uvs = new List<Vector2>();
        GetComponent<MeshFilter>().mesh.GetUVs(0, uvs);
        GetComponent<MeshFilter>().mesh.SetUVs(0, uvs);
    }
}
