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
    public bool IsInteriorCeiling { get; private set; }

    public override void Populate(IDictionary<string, string> properties) {
        transformChild = new GameObject("Wall3D");
        transformChild.transform.parent = transform;

        Z = properties.ContainsKey("z") ? float.Parse(properties["z"]) : 0.0f;
        IsInteriorCeiling = properties.ContainsKey("ceil") && properties["ceil"].Equals("interior");

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

                bool floorExistsN = FloorExistsAt(new IntVector2(x, y - 1));
                bool floorExistsS = FloorExistsAt(new IntVector2(x, y + 1));
                bool floorExistsE = FloorExistsAt(new IntVector2(x + 1, y));
                bool floorExistsW = FloorExistsAt(new IntVector2(x - 1, y));

                LinkedTileset linkedTileset = tiledMap.GetTilesetForTileId(nativeTileId);
                tileIds.Insert(0, nativeTileId - linkedTileset.firstGid);
                for (int z = 0; z < tileIds.Count; z += 1) {
                    int tileId = tileIds[z];
                    TiledProperty prefabNameProperty = linkedTileset.tileset.PropertyForTile(tileId, "prefab");
                    string prefabName = prefabNameProperty == null ? "WallChunk" : prefabNameProperty.GetStringValue();
                    prefabName = "Prefabs/Map3D/" + prefabName;

                    Wall3D wallChunk = Instantiate<GameObject>(Resources.Load<GameObject>(prefabName)).GetComponent<Wall3D>();
                    wallChunk.transform.parent = transformChild.transform;
                    wallChunk.transform.position = new Vector3(x, z, -1 * y - 1);

                    foreach (TileMeshRenderer side in wallChunk.GetAllSides()) {
                        side.AssignTileId(tiledMap, linkedTileset.tileset, tileId);
                    }
                    if (!floorExistsN) DestroyImmediate(wallChunk.North.gameObject);
                    if (!floorExistsS) DestroyImmediate(wallChunk.South.gameObject);
                    if (!floorExistsE) DestroyImmediate(wallChunk.East.gameObject);
                    if (!floorExistsW) DestroyImmediate(wallChunk.West.gameObject);
                }
            }
        }

        transformChild.transform.localEulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
    }

    private bool FloorExistsAt(IntVector2 coords) {
        TiledMap tiledMap = transform.parent.GetComponentInParent<TiledMap>();
        if (coords.x < 0 || coords.x >= tiledMap.NumTilesWide) {
            return false;
        }
        if (coords.y < 0 || coords.y >= tiledMap.NumTilesHigh) {
            return false;
        }
        int terrainId = GetComponent<TileLayer>().TerrainIds[coords.y * tiledMap.NumTilesWide + coords.x];
        if (terrainId == 0) {
            return false;
        }
        return tiledMap.GetPropertyForTile("x", GetComponent<TileLayer>(), coords.x, coords.y) == null;
    }
}
