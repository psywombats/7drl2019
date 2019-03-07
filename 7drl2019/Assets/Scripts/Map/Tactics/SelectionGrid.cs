using UnityEngine;
using System;
using System.Collections.Generic;

/**
 *  A purely cosmetic grid that can be configured to display tiles as 'on' or 'off.' It then renders
 *  each cell differently depending on its on/off state. Can also be queried as to the state of a
 *  cell so can control things like "should the cursor extend outside of this zone."
 */
public class SelectionGrid : MonoBehaviour {

    const string InstancePath = "Prefabs/Tactics/SelectionGrid";

    // editor properties
    public MeshFilter mesh;
    public MeshRenderer meshRenderer;

    public bool cameraFollows { get; set; }

    enum TileType {
        SolidN, SolidE, SolidS, SolidW,
        BorderN, BorderE, BorderS, BorderW,
        CornerNW, CornerNE, CornerSE, CornerSW,
        ElbowNW, ElbowNE, ElbowSE, ElbowSW,
    }

    public static SelectionGrid GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<SelectionGrid>();
    }

    // set up a new grid with the given size in tiles and rule for turning location into whether a
    // tile is part of the selection grid or not
    public void ConfigureNewGrid(Vector2Int at, int range, TacticsTerrainMesh terrain,
            Func<Vector2Int, bool> rangeRule, Func<Vector2Int, bool> selectRule) {
        ConfigureNewGrid(at - new Vector2Int(range, range), new Vector2Int(range * 2 + 1, range * 2 + 1), terrain, 
            rangeRule, selectRule);
    }

    public void ConfigureNewGrid(Vector2Int at, Vector2Int size, TacticsTerrainMesh terrain, 
            Func<Vector2Int, bool> rangeRule, Func<Vector2Int, bool> selectRule) {
        transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        LineOfSightEffect los = terrain.GetComponent<LineOfSightEffect>();

        MeshFilter filter = this.mesh;
        if (filter.mesh != null) {
            Destroy(filter.mesh);
        }
        Mesh mesh = new Mesh();
        filter.mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        for (int y = at.y; y < at.y + size.y; y += 1) {
            for (int x = at.x; x < at.x + size.x; x += 1) {
                if (terrain.HeightAt(x, y) == 0.0f || !los.CachedIsVisible(new Vector2Int(x, y))) {
                    continue;
                }
                bool selectable = selectRule(new Vector2Int(x, y));
                bool rangeable = rangeRule(new Vector2Int(x, y));
                if (selectable || rangeable) {
                    int vertIndex = vertices.Count;
                    float height = terrain.HeightAt(x, y);
                    for (int i = 0; i < 4; i += 1) {
                        if (selectable) {
                            uvs.Add(new Vector2(
                                i % 2 == 0 ? .5f : 0,
                                i < 2 ? .5f : 0));
                        } else if (rangeable) {
                            uvs.Add(new Vector2(
                                i % 2 == 0 ? 1 : .5f,
                                i < 2 ? .5f : 0));
                        }
                        vertices.Add(new Vector3(
                            x + (i % 2 == 0 ? 1 : 0),
                            height,
                            y + (i < 2 ? 1 : 0)));
                    }
                    tris.Add(vertIndex);
                    tris.Add(vertIndex + 2);
                    tris.Add(vertIndex + 1);
                    tris.Add(vertIndex + 1);
                    tris.Add(vertIndex + 2);
                    tris.Add(vertIndex + 3);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
    }
}
