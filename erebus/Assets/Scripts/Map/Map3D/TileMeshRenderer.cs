using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;

[RequireComponent(typeof(MeshRenderer))]
public class TileMeshRenderer : MonoBehaviour {

    public void AssignTileId(TiledMap map, int tileId) {
        LinkedTileset linkedTileset = map.GetTilesetForTileId(tileId);
        Tileset tileset = linkedTileset.tileset;

        int width = map.gameObject.GetComponent<Map>().WidthPx;
        int realGid = tileId = linkedTileset.firstGid;
        
    }
}
