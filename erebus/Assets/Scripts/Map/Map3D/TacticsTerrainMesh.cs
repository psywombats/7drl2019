using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TacticsTerrainMesh : MonoBehaviour {
    
    public Vector2Int size;
    [HideInInspector]
    public float[] heights;
    [HideInInspector]
    public Tile[] topTiles;
    public Tile defaultTile;
    [HideInInspector]
    public string paletteName;

    public float HeightAt(int x, int y) {
        if (heights == null || heights.Length == 0) {
            heights = new float[3* 3];
            for (int yy = 0; yy < 3; yy += 1) {
                for (int xx = 0; xx < 3; xx += 1) {
                    heights[xx + yy * size.y] = 0.5f + 0.5f * (xx + yy);
                }
            }
        }
        if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
            return 0;
        } else {
            return heights[y * size.y + x];
            
        }
    }

    public void SetHeight(int x, int y, float height) {
        heights[y * size.y + x] = height;
    }

    public Tile TileAt(int x, int y) {
        if (topTiles == null) {
            topTiles = new Tile[size.x * size.y];
        }
        Tile tile = topTiles[y * size.y + x];
        if (tile == null) {
            return defaultTile;
        } else {
            return tile;
        }
    }

    public void SetTile(int x, int y, Tile tile) {
        topTiles[y * size.y + x] = tile;
    }
}
