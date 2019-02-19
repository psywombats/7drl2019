﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TacticsTerrainMesh : MonoBehaviour, ISerializationCallbackReceiver {
    
    [HideInInspector]
    public Vector2Int size;
    [HideInInspector]
    public float[] heights;
    [HideInInspector]
    public Tile[] topTiles;
    public Tile defaultTile;
    [HideInInspector]
    public string paletteName;

    [HideInInspector]
    public FacingTileDictionary serializedFacingTiles;
    private Dictionary<FacingTileKey, Tile> facingTiles;

    public void Resize(Vector2Int newSize) {
        foreach (FacingTileKey key in facingTiles.Keys) {
            if (key.pos.x >= newSize.x || key.pos.z >= newSize.y) {
                facingTiles.Remove(key);
            }
        }

        float[] newHeights = new float[newSize.x * newSize.y];
        Tile[] newTiles = new Tile[newSize.x * newSize.y];
        for (int y = 0; y < (size.y < newSize.y ? size.y : newSize.y); y += 1) {
            for (int x = 0; x < (size.x < newSize.x ? size.x : newSize.x); x += 1) {
                newHeights[y * newSize.x + x] = heights[y * size.x + x];
                newTiles[y * newSize.x + x] = topTiles[y * size.x + x];
            }
        }
        topTiles = newTiles;
        heights = newHeights;
        size = newSize;
    }

    public float HeightAt(int x, int y) {
        if (heights == null || heights.Length == 0) {
            heights = new float[3* 3];
            for (int yy = 0; yy < 3; yy += 1) {
                for (int xx = 0; xx < 3; xx += 1) {
                    heights[xx + yy * size.x] = 0.5f + 0.5f * (xx + yy);
                }
            }
        }
        if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
            return 0;
        } else {
            return heights[y * size.x + x];
            
        }
    }

    public void SetHeight(int x, int y, float height) {
        heights[y * size.x + x] = height;
    }

    public Tile TileAt(int x, int y) {
        if (topTiles == null) {
            topTiles = new Tile[size.x * size.y];
        }
        Tile tile = topTiles[y * size.x + x];
        if (tile == null) {
            return defaultTile;
        } else {
            return tile;
        }
    }

    public Tile TileAt(int x, int y, float height, OrthoDir dir) {
        FacingTileKey key = new FacingTileKey();
        key.dir = dir;
        key.pos = new Vector3(x, height, y);
        if (facingTiles.ContainsKey(key)) {
            return facingTiles[key];
        } else {
            return defaultTile;
        }
    }

    public void SetTile(int x, int y, Tile tile) {
        topTiles[y * size.y + x] = tile;
    }

    public void SetTile(int x, int y, float height, OrthoDir dir, Tile tile) {
        FacingTileKey key = new FacingTileKey();
        key.dir = dir;
        key.pos = new Vector3(x, height, y);
        facingTiles[key] = tile;
    }

    // === SERIALIZATION ===========================================================================

    public void OnBeforeSerialize() {
        serializedFacingTiles = new FacingTileDictionary(facingTiles);
    }

    public void OnAfterDeserialize() {
        if (serializedFacingTiles != null) {
            facingTiles = serializedFacingTiles.ToDictionary();
        } else {
            facingTiles = new Dictionary<FacingTileKey, Tile>();
        }
    }

    [System.Serializable]
    public struct FacingTileKey {
        public Vector3 pos;
        public OrthoDir dir;
    }

    [System.Serializable]
    public class FacingTileDictionary : SerialDictionary<FacingTileKey, Tile> {
        public FacingTileDictionary(Dictionary<FacingTileKey, Tile> dictionary) : base(dictionary) {

        }
    }
}
