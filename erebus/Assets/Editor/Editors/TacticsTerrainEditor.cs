using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TacticsTerrainMesh))]
public class TacticsTerrainEditor : Editor {

    private List<TerrainQuad> quads;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> tris;
    private TerrainQuad lastSelected;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Rebuild")) {
            Rebuild(true);
        }
    }

    public void OnSceneGUI() {
        if (Event.current.type == EventType.MouseMove) {
            Debug.Log("Mouse moved to " + Event.current.mousePosition);
            if (quads == null) {
                Rebuild(false);
            }
            TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
            TerrainQuad quad = GetSelectedQuad();
            if (lastSelected != quad) {
                SceneView.RepaintAll();
                lastSelected = quad;
                if (quad != null) {
                    terrain.selectedNormal = quad.normal;
                    terrain.selectedPos = quad.pos;
                } else {
                    terrain.selectedNormal = Vector2.zero;
                    terrain.selectedPos = Vector3.zero;
                }
            }
        }
    }

    private void Rebuild(bool regenMesh) {
        Debug.Log("rebuidling...");
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        MeshFilter filter = terrain.GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            AssetDatabase.CreateAsset(mesh, "Assets/Resources/Meshes/" + UnityEngine.Random.Range(10000000, 99999999) + ".asset");
            filter.sharedMesh = mesh;
        }

        quads = new List<TerrainQuad>();
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        tris = new List<int>();
        for (int z = 0; z < terrain.size.y; z += 1) {
            for (int x = 0; x < terrain.size.x; x += 1) {
                // top vertices
                float height = terrain.HeightAt(x, z);
                AddQuad(new Vector3(x, height, z), new Vector3(x + 1, height, z + 1), terrain.TileAt(x, z),
                    new Vector3(x, height, z), new Vector3(0, 1, 0));

                // side vertices
                foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
                    float currentHeight = terrain.HeightAt(x, z);
                    float neighborHeight = terrain.HeightAt(x + dir.Px3DX(), z + dir.Px3DZ());
                    if (currentHeight > neighborHeight) {
                        Vector2 off1 = Vector2.zero, off2 = Vector2.zero;
                        switch (dir) {
                            case OrthoDir.South:
                                off1 = new Vector2(0, 0);
                                off2 = new Vector2(1, 0);
                                break;
                            case OrthoDir.East:
                                off1 = new Vector2(1, 1);
                                off2 = new Vector2(1, 0);
                                break;
                            case OrthoDir.North:
                                off1 = new Vector2(1, 1);
                                off2 = new Vector2(0, 1);
                                break;
                            case OrthoDir.West:
                                off1 = new Vector2(0, 0);
                                off2 = new Vector2(0, 1);
                                break;
                        }
                        for (float y = neighborHeight; y < currentHeight; y += 0.5f) {
                            AddQuad(new Vector3(x + off1.x, y, z + off1.y),
                                new Vector3(x + off2.x, y + 0.5f, z + off2.y),
                                (PropertiedTile)terrain.defaultTile,
                                new Vector3(x, y + 0.5f, z), dir.Px3D());
                        }
                    }
                }
            }
        }

        if (regenMesh) {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateBounds();
        }
    }

    private void AddQuad(Vector3 lowerLeft, Vector3 upperRight, PropertiedTile tile, Vector3 pos, Vector3 normal) {
        TerrainQuad quad = new TerrainQuad(tris.Count, vertices.Count, pos, normal);
        quads.Add(quad);

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

        if (tile != null) {
            Vector2[] spriteUVs = tile.GetSprite().uv;
            uvs.Add(spriteUVs[0]);
            uvs.Add(spriteUVs[1]);
            uvs.Add(spriteUVs[2]);
            uvs.Add(spriteUVs[3]);
        }

        vertices.Add(upperRight);
        tris.Add(i);
        tris.Add(i + 1);
        tris.Add(i + 2);
        tris.Add(i + 1);
        tris.Add(i + 3);
        tris.Add(i + 2);
    }

    private TerrainQuad GetSelectedQuad() {
        if (quads == null) {
            return null;
        }

        Vector2 mousePos = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        float bestT = -1.0f;
        TerrainQuad best = null;
        foreach (TerrainQuad quad in quads) {
            float t1 = EditorUtils.IntersectTri(ray, 
                vertices[tris[quad.trisIndex + 0]], 
                vertices[tris[quad.trisIndex + 1]], 
                vertices[tris[quad.trisIndex + 2]]);
            float t2 = EditorUtils.IntersectTri(ray,
                vertices[tris[quad.trisIndex + 3]],
                vertices[tris[quad.trisIndex + 4]],
                vertices[tris[quad.trisIndex + 5]]);
            if ((t1 <= t2 || t2 == -1) && t1 > 0 && (t1 < bestT || bestT == -1.0f)) {
                best = quad;
                bestT = t1;
            } else if (t2 > 0 && (t2 < bestT || bestT == -1.0f)) {
                best = quad;
                bestT = t2;
            }
        }

        return best;
    }
}
