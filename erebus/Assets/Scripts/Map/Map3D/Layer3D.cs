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

    public GameObject transformChild;
    public float Z { get; set; }

    public override void Populate(IDictionary<string, string> properties) {
        transformChild = new GameObject("Wall3D");
        transformChild.transform.parent = transform;

        Z = properties.ContainsKey("z") ? float.Parse(properties["z"]) : 0.0f;

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
                    int tileId = tileIds[z];
                    TiledProperty prefabNameProperty = linkedTileset.tileset.PropertyForTile(tileId, "prefab");
                    string prefabName = prefabNameProperty == null ? "WallChunk" : prefabNameProperty.GetStringValue();
                    prefabName = "Prefabs/Map3D/" + prefabName;

                    GameObject wallChunk = Instantiate<GameObject>(Resources.Load<GameObject>(prefabName));
                    wallChunk.transform.parent = transformChild.transform;
                    wallChunk.transform.position = new Vector3(x, z, -1 * y - 1);
                    
                    Wall3D wall = wallChunk.GetComponent<Wall3D>();
                    foreach (TileMeshRenderer side in wall.GetAllSides()) {
                        side.AssignTileId(tiledMap, linkedTileset.tileset, tileId);
                    }
                }
            }
        }

        transformChild.transform.localEulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
    }
}
