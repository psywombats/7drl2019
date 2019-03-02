using UnityEngine;
using System.Collections.Generic;

public struct MeshGenerationResult {

    public Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>> quads;
    public List<Vector3> vertices;
    public List<Vector2> uvs;
    public List<int> tris;
}
