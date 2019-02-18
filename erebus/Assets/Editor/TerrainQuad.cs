using UnityEngine;

public class TerrainQuad {

    // we occupy the six triangles in the tris array starting here
    public int trisIndex;

    // we occupy the four vertices in the vert array starting here
    // can also index into the uvs array
    public int vertsIndex;

    public Vector3 pos;
    public Vector3 normal;

    public TerrainQuad(int trisIndex, int vertsIndex, Vector3 pos, Vector3 normal) {
        this.trisIndex = trisIndex;
        this.vertsIndex = vertsIndex;
        this.pos = pos;
        this.normal = normal;
    }
}
