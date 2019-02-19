using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainQuad {

    // we occupy the six triangles in the tris array starting here
    public int trisIndex;

    // we occupy the four vertices in the vert array starting here
    // can also index into the uvs array
    public int vertsIndex;

    public Vector3 pos;
    public Vector3 normal;

    // creating a new quad mutates the tri/vert/uv arrays
    public TerrainQuad(List<int> tris, List<Vector3> vertices, List<Vector2> uvs,
            Vector3 lowerLeft, Vector3 upperRight, 
            Tile tile, Tilemap tileset, Vector3 pos, Vector3 normal) {
        trisIndex = tris.Count;
        vertsIndex = vertices.Count;
        this.pos = pos;
        this.normal = normal;

        int i = vertices.Count;
        vertices.Add(lowerLeft);
        if (lowerLeft.x == upperRight.x) {
            vertices.Add(new Vector3(lowerLeft.x, lowerLeft.y, upperRight.z));
            vertices.Add(new Vector3(lowerLeft.x, upperRight.y, lowerLeft.z));
        } else if (lowerLeft.y == upperRight.y) {
            vertices.Add(new Vector3(lowerLeft.x, lowerLeft.y, upperRight.z));
            vertices.Add(new Vector3(upperRight.x, lowerLeft.y, lowerLeft.z));
        } else if (lowerLeft.z == upperRight.z) {
            vertices.Add(new Vector3(lowerLeft.x, upperRight.y, lowerLeft.z));
            vertices.Add(new Vector3(upperRight.x, lowerLeft.y, lowerLeft.z));
        }
        vertices.Add(upperRight);

        Debug.Assert(tile != null);
        Vector2[] spriteUVs = tile.sprite.uv;
        if (normal.y == 0.0f) {
            spriteUVs = MathHelper3D.AdjustZ(spriteUVs, tileset, lowerLeft.y, normal.x == 0.0f);
        }
        uvs.Add(spriteUVs[2]);
        uvs.Add(spriteUVs[0]);
        uvs.Add(spriteUVs[3]);
        uvs.Add(spriteUVs[1]);

        tris.Add(i);
        tris.Add(i + 1);
        tris.Add(i + 2);
        tris.Add(i + 1);
        tris.Add(i + 3);
        tris.Add(i + 2);
    }
}
