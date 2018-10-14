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

    enum TileType {
        SolidN,     SolidE,     SolidS,     SolidW,
        BorderN,    BorderE,    BorderS,    BorderW,
        CornerNW,   CornerNE,   CornerSE,   CornerSW,
        ElbowNW,    ElbowNE,    ElbowSE,    ElbowSW,
    }

    public void OnEnable() {
        ConfigureNewGrid(new IntVector2(5, 5), (IntVector2 loc) => {
            return loc.x >= loc.y;
        });
    }

    // set up a new grid with the given size in tiles and rule for turning location into whether a
    // tile is part of the selection grid or not
    public void ConfigureNewGrid(IntVector2 size, Func<IntVector2, Boolean> rule) {
        this.size = size;
        this.rule = rule;
        RecalculateGrid();
    }

    // redo this grid based on a new rule
    // assumes size has already been configured
    public void RecalculateRule(Func<IntVector2, Boolean> rule) {
        this.rule = rule;
        IntVector2 gridSize = this.size * 2;
        Mesh mesh = this.Mesh.mesh;

        // we'll need to call the rule on each square anyway so just do it once per
        bool[,] ruleGrid = new Boolean[size.x, size.y];
        for (int y = 0; y < size.y; y += 1) {
            for (int x = 0; x < size.x; x += 1) {
                ruleGrid[x, y] = rule(new IntVector2(x, y));
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

        // now that we have geometry, let's texture it with uvs
        Vector2[] uvs = new Vector2[(gridSize.x + 1) * (gridSize.y + 1)];
        for (int y = 0; y < gridSize.y; y += 1) {
            for (int x = 0; x < gridSize.x; x += 1) {
                if (!CheckIfEnabledAt(ruleGrid, x, y)) {
                    continue;
                }
                int tileX = x / 2;
                int tileY = y / 2;
                int tileN = (y - 1) / 2;
                int tileS = (y + 1) / 2;
                int tileE = (x - 1) / 2;
                int tileW = (x + 1) / 2;
                bool checkN = !ruleGrid[tileX, tileN];
                bool checkS = !ruleGrid[tileX, tileS];
                bool checkE = !ruleGrid[tileE, tileY];
                bool checkW = !ruleGrid[tileW, tileY];

                if (checkN) {
                    if (checkE) {
                        FillUV(uvs, x, y, TileType.CornerNE);
                    } else if (checkW) {
                        FillUV(uvs, x, y, TileType.CornerNW);
                    } else {
                        FillUV(uvs, x, y, TileType.BorderN);
                    }
                } else if (checkS) {
                    if (checkE) {
                        FillUV(uvs, x, y, TileType.CornerSE);
                    } else if (checkW) {
                        FillUV(uvs, x, y, TileType.CornerSW);
                    } else {
                        FillUV(uvs, x, y, TileType.BorderS);
                    }
                } else if (checkE) {
                    FillUV(uvs, x, y, TileType.BorderE);
                } else if (checkW) {
                    FillUV(uvs, x, y, TileType.BorderW);
                } else if (!ruleGrid[x - 1, y - 1]) {
                    FillUV(uvs, x, y, TileType.ElbowNW);
                } else if (!ruleGrid[x - 1, y + 1]) {
                    FillUV(uvs, x, y, TileType.ElbowSW);
                } else if (!ruleGrid[x + 1, y - 1]) {
                    FillUV(uvs, x, y, TileType.ElbowNE);
                } else if (!ruleGrid[x + 1, y + 1]) {
                    FillUV(uvs, x, y, TileType.ElbowSE);
                } else {
                    if (x % 2 == 0) {
                        if (y % 2 == 0) {
                            return FillUV(uvs, x, y, TileType.SolidN);
                        }
                    }
                }

                i += 1;
            }
        }
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

        RecalculateRule(this.rule);
    }

    // evaluates ruleGrid at (x/2, y/2) with bounds checking
    private bool CheckIfEnabledAt(bool[,] ruleGrid, int x, int y) {
        int tileX = x / 2;
        int tileY = y = 2;
        if (tileX < 0 || tileX >= size.x) {
            return false;
        }
        if (tileY < 0 || tileY >= size.y) {
            return false;
        }
        return ruleGrid[x, y];
    }

    private void FillUV(Vector2[] uvs, int x, int y, TileType tile) {

    }
}
