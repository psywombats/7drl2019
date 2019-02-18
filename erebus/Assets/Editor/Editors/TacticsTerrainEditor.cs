using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TacticsTerrainMesh))]
public class TacticsTerrainEditor : Editor {

    private List<Vector3> vertices;
    private List<int> tris;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Rebuild")) {
            Rebuild();
        }
    }

    public void Rebuild() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        MeshFilter filter = terrain.GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            AssetDatabase.CreateAsset(mesh, "Assets/Resources/Meshes/" + UnityEngine.Random.Range(10000000, 99999999) + ".asset");
            filter.sharedMesh = mesh;
        }

        vertices = new List<Vector3>();
        tris = new List<int>();
        for (int z = 0; z < terrain.size.y; z += 1) {
            for (int x = 0; x < terrain.size.x; x += 1) {
                // top vertices
                float height = terrain.HeightAt(x, z);
                AddTrisForQuad(new Vector3(x, height, z), new Vector3(x + 1, height, z + 1));

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
                        AddTrisForQuad(new Vector3(x + off1.x, neighborHeight, z + off1.y), 
                            new Vector3(x + off2.x, currentHeight, z + off2.y));
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateBounds();
    }

    private void AddTrisForQuad(Vector3 lowerLeft, Vector3 upperRight) {
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
        tris.Add(i);
        tris.Add(i + 1);
        tris.Add(i + 2);
        tris.Add(i + 1);
        tris.Add(i + 3);
        tris.Add(i + 2);
    }
}
