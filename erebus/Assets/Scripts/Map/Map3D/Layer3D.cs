using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * 3D map class for Tiled maps that get converted into dungeon crawl style scenes
 */
[RequireComponent(typeof(TiledMap))]
public class Layer3D : TiledInstantiated {

    public override void Populate(IDictionary<string, string> properties) {
        TiledMap tiledMap = transform.parent.GetComponentInParent<TiledMap>();
        for (int x = 0; x < tiledMap.NumTilesWide; x += 1) {
            for (int y = 0; y < tiledMap.NumTilesHigh; y += 1) {
                TiledProperty wallSeqProperty = tiledMap.GetPropertyForTile("wall3d", GetComponent<TileLayer>(), x, y);
                if (wallSeqProperty == null) {
                    continue;
                }

                string wallSeq = wallSeqProperty.GetStringValue();
                string[] splitSeq = wallSeq.Split(new Char[] { ',' });

                List<int> tileIds = new List<int>();
                int nativeTileId = GetComponent<TileLayer>().TerrainIds[x + y * tiledMap.NumTilesWide];
                foreach (string stringId in splitSeq) {
                    tileIds.Add(int.Parse(stringId));
                }

                LinkedTileset linkedTileset = tiledMap.GetTilesetForTileId(nativeTileId);
                tileIds.Insert(0, nativeTileId - linkedTileset.firstGid);
                for (int z = 0; z < tileIds.Count; z += 1) {
                    GameObject wallChunk = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Map3D/WallChunk"));
                    wallChunk.transform.parent = gameObject.transform;
                    Wall3D wall = wallChunk.GetComponent<Wall3D>();
                    foreach (TileMeshRenderer side in wall.GetAllSides()) {
                        side.AssignTileId(tiledMap, linkedTileset.tileset, tileIds[z]);
                    }
                }
            }
        }
    }
}
