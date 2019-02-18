using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TacticsTerrainMesh : MonoBehaviour {
    
    public Vector2Int size;
    public float[,] heights;

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
}
