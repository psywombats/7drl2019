using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TacticsTerrainMesh : MonoBehaviour {
    
    public Vector2Int size;
    public float[,] heights;
    public PropertiedTile[,] topTiles;
    public Tile defaultTile;

    public float HeightAt(int x, int y) {
        if (heights == null || heights.Length == 0) {
            heights = new float[3, 3];
            for (int yy = 0; yy < 3; yy += 1) {
                for (int xx = 0; xx < 3; xx += 1) {
                    heights[xx, yy] = 0.5f + 0.5f * (xx + yy);
                }
            }
        }
        if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
            return 0;
        } else {
            return heights[x, y];
            
        }
    }

    public PropertiedTile TileAt(int x, int y) {
        //PropertiedTile tile = topTiles[x, y];
        //if (tile == null) {
        //    return defaultTile;
        //} else {
        //    return tile;
        //}
        return (PropertiedTile)defaultTile;
    }
}
