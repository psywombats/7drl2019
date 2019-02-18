using UnityEngine;
using System;
using UnityEngine.Profiling;

/**
 *  A purely cosmetic grid that can be configured to display tiles as 'on' or 'off.' It then renders
 *  each cell differently depending on its on/off state. Can also be queried as to the state of a
 *  cell so can control things like "should the cursor extend outside of this zone."
 */
[ExecuteInEditMode]
public class SelectionGrid : MonoBehaviour {

    const string InstancePath = "Prefabs/Map3D/SelectionGrid";

    // editor properties
    public MeshFilter mesh;
    public MeshRenderer meshRenderer;

    private Vector2Int size;
    private Func<Vector2Int, bool> rule;

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

    public void OnEnable() {
        ConfigureNewGrid(new Vector2Int(5, 3), (Vector2Int loc) => {
            return loc.x > 0 || loc.y > 0;
        });
    }

    // set up a new grid with the given size in tiles and rule for turning location into whether a
    // tile is part of the selection grid or not
    public void ConfigureNewGrid(Vector2Int size, Func<Vector2Int, bool> rule) {
        this.size = size;
        this.rule = rule;
        RecalculateGrid();
    }

    // redo this grid based on a new rule
    // assumes size has already been configured
    public void RecalculateRule(Func<Vector2Int, bool> rule) {
        this.rule = rule;
        Vector2Int gridSize = size * 2;
        Mesh mesh = this.mesh.mesh;

        // we'll need to call the rule on each square anyway so just do it once per
        bool[,] ruleGrid = new bool[size.x, size.y];
        for (int y = 0; y < size.y; y += 1) {
            for (int x = 0; x < size.x; x += 1) {
                ruleGrid[x, y] = rule(new Vector2Int(x, y));
            }
        }
        
        // redo the triangle geometry to only reflect where rule evaluates true
        int[] triangles = new int[gridSize.x * gridSize.y * 6];
        for (int ti = 0, vi = 0, y = 0; y < gridSize.y; y++, vi++) {
            for (int x = 0; x < gridSize.x; x++, ti += 6, vi++) {
                // integer division to figure out the tile we're evaluating
                int tileX = x / 2;
                int tileY = y / 2;
                if (!ruleGrid[tileX, tileY]) {
                    continue;
                }
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + gridSize.x + 1;
                triangles[ti + 5] = vi + gridSize.x + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void RecalculateGrid() {
        // we now have a rule and a size, update the mesh texture to reflect this
        Vector2Int gridSize = size * 2;

        MeshFilter filter = this.mesh;
        if (filter.mesh != null) {
            Destroy(filter.mesh);
        }
        Mesh mesh = new Mesh();
        filter.mesh = mesh;

        Vector3[] vertices = new Vector3[(gridSize.x + 1) * (gridSize.y + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int i = 0;
        for (int y = 0; y <= gridSize.y; y += 1) {
            for (int x = 0; x <= gridSize.x; x += 1) {
                vertices[i] = new Vector3(0.5f * (float)x, 0.5f * (float)y, 0.0f);
                uvs[i] = new Vector2((float)x / 2.0f, (float)y / 2.0f);
                i += 1;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;

        RecalculateRule(rule);
    }
}
