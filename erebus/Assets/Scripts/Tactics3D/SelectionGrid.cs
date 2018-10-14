using UnityEngine;
using System.Collections;
using System;

/**
 *  A purely cosmetic grid that can be configured to display tiles as 'on' or 'off.' It then renders
 *  each cell differently depending on its on/off state. Can also be queried as to the state of a
 *  cell so can control things like "should the cursor extend outside of this zone."
 */
 [ExecuteInEditMode]
public class SelectionGrid : MonoBehaviour {

    public MeshFilter Mesh;
    public MeshRenderer MeshRenderer;
    [Header("Autotile texture - RM style")]
    public Texture2D GridTexture;

    private IntVector2 size;
    private Func<IntVector2, Boolean> rule;

    public void OnEnable() {
        ConfigureNewGrid(new IntVector2(2, 2), (IntVector2 loc) => {
            return true;
        });
    }

    // set up a new grid with the given size in tiles and rule for turning location into whether a
    // tile is part of the selection grid or not
    public void ConfigureNewGrid(IntVector2 size, Func<IntVector2, Boolean> rule) {
        this.size = size;
        this.rule = rule;
        RecalculateGrid();
    }

    private void RecalculateGrid() {
        // we now have a rule and a size, update the mesh texture to reflect this

        IntVector2 gridSize = this.size * 2;

        MeshFilter filter = this.Mesh;
        if (filter.mesh != null) {
            Destroy(filter.mesh);
        }
        Mesh mesh = new Mesh();
        filter.mesh = mesh;

        Vector3[] vertices = new Vector3[(gridSize.x + 1) * (gridSize.y + 1)];
        int i = 0;
        for (int y = 0;  y <= gridSize.y; y += 1) {
            for (int x = 0; x <= gridSize.x; x += 1) {
                vertices[i] = new Vector3(0.5f * (float)x, 0.5f * (float)y, 0.0f);
                i += 1;
            }
        }
        mesh.vertices = vertices;

        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = (gridSize.x + 1) * gridSize.y;
        triangles[2] = gridSize.x;
        triangles[3] = gridSize.x;
        triangles[4] = (gridSize.x + 1) * gridSize.y;
        triangles[5] = (gridSize.x + 1) * (gridSize.y + 1) - 1;
        mesh.triangles = triangles;
    }
}
