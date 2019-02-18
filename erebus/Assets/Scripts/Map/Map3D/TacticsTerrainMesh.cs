using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TacticsTerrainMesh : MonoBehaviour {
    
    public Vector2Int size;
    public float[,] heights;
    public PropertiedTile[,] topTiles;
    public Tile defaultTile;
    
    public Vector3 selectedPos;
    public Vector3 selectedNormal;

    public void OnDrawGizmos() {
        if (selectedNormal != Vector3.zero) {
            Vector3 mid = selectedPos + new Vector3(0.5f, -0.25f, 0.5f) + new Vector3(
                selectedNormal.x * 0.5f,
                selectedNormal.y * 0.25f,
                selectedNormal.z * 0.5f);
            Vector3 size = new Vector3(
                1.01f - Mathf.Abs(selectedNormal.x), 
                0.51f - (Mathf.Abs(selectedNormal.y) * 0.5f), 
                1.01f - Mathf.Abs(selectedNormal.z));
            Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 0.85f);
            Gizmos.DrawCube(mid, size);
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f);
            Gizmos.DrawWireCube(mid, size);
        }
    }

    public bool FilledAt(int x, int y, int z) {
        return heights[x, y] >= z;
    }

    public float HeightAt(int x, int y) {
        if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
            return 0;
        } else {
            //return heights[x, y];
            return 0.5f * (x + y) + 0.5f;
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
