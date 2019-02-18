using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TacticsTerrainMesh))]
public class TacticsTerrainEditor : Editor {

    private enum EditMode {
        None,
        HeightAdjust,
    }

    private List<TerrainQuad> quads;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> tris;

    private TerrainQuad lastSelected;
    private float selectedHeight;

    private EditMode mode = EditMode.None;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Rebuild")) {
            Rebuild(true);
        }
    }

    public void OnGUI() {
        switch (Event.current.commandName) {
            case "UndoRedoPerformed":
                Rebuild(false);
                break;
        }
    }

    public void OnSceneGUI() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        if (quads == null) {
            Rebuild(false);
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.MouseMove:
                switch (mode) {
                    case EditMode.None:
                        TerrainQuad quad = GetSelectedQuad();
                        if (lastSelected != quad) {
                            SceneView.RepaintAll();
                            lastSelected = quad;
                        }
                        break;
                }
                break;
            case EventType.MouseDrag:
                switch (mode) {
                    case EditMode.HeightAdjust:
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        selectedHeight = GetHeightAtMouse();
                        break;
                }
                break;
            case EventType.MouseDown:
                if (mode == EditMode.None) {
                    lastSelected = GetSelectedQuad();
                    if (lastSelected != null && lastSelected.normal.y > 0.0f) {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        mode = EditMode.HeightAdjust;
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        selectedHeight = 0.0f;
                    }
                }
                break;
            case EventType.MouseUp:
                switch (mode) {
                    case EditMode.HeightAdjust:
                        GUIUtility.hotControl = 0;
                        Event.current.Use();

                        mode = EditMode.None;

                        float height = GetHeightAtMouse();
                        int x = Mathf.RoundToInt(lastSelected.pos.x);
                        int y = Mathf.RoundToInt(lastSelected.pos.z);
                        if (terrain.heights[x, y] != height) {
                            terrain.heights[x, y] = height;
                            Undo.RecordObject(terrain, "Modify terrain height");
                            Rebuild(true);
                        }
                        break;
                }
                break;
        }

        if (selectedHeight > 0.0f && mode == EditMode.HeightAdjust) {
            int x = Mathf.RoundToInt(lastSelected.pos.x);
            int y = Mathf.RoundToInt(lastSelected.pos.z);
            float h = terrain.HeightAt(x, y);
            Vector3 pos = lastSelected.pos;
            for (float z = GetHeightAtMouse(); Mathf.Abs(z - h) > 0.1f; z += 0.5f * Mathf.Sign(h - z)) {
                Handles.DrawWireCube(new Vector3(pos.x + 0.5f, z + 0.25f * Mathf.Sign(h - z), pos.z + 0.5f),
                    new Vector3(1.0f, 0.5f, 1.0f));
            }
        }
        if (mode == EditMode.None || ( mode == EditMode.HeightAdjust)) {
            if (lastSelected != null) {
                Vector3 mid = lastSelected.pos + new Vector3(0.5f, -0.25f, 0.5f) + new Vector3(
                    lastSelected.normal.x * 0.5f,
                    lastSelected.normal.y * 0.25f,
                    lastSelected.normal.z * 0.5f);
                Vector3 size = new Vector3(
                    1.01f - Mathf.Abs(lastSelected.normal.x),
                    0.51f - (Mathf.Abs(lastSelected.normal.y) * 0.5f),
                    1.01f - Mathf.Abs(lastSelected.normal.z));
                Handles.color = new Color(1.0f, 1.0f, 1.0f);
                Handles.DrawWireCube(mid, size);
            }
        }
    }

    private void Rebuild(bool regenMesh) {
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
            lastSelected = null;

            mesh.Clear();
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
        vertices.Add(upperRight);

        Debug.Assert(tile != null);
        Vector2[] spriteUVs = tile.GetSprite().uv;
        uvs.Add(spriteUVs[0]);
        uvs.Add(spriteUVs[1]);
        uvs.Add(spriteUVs[2]);
        uvs.Add(spriteUVs[3]);
        
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
            float t = RayDistanceForQuad(ray, quad);
            if (t > 0.0f && (t < bestT || bestT == -1.0f)) {
                bestT = t;
                best = quad;
            }
        }

        return best;
    }

    private float RayDistanceForQuad(Ray ray, TerrainQuad quad) {
        float t1 = EditorUtils.IntersectTri(ray,
            vertices[tris[quad.trisIndex + 0]],
            vertices[tris[quad.trisIndex + 1]],
            vertices[tris[quad.trisIndex + 2]]);
        float t2 = EditorUtils.IntersectTri(ray,
            vertices[tris[quad.trisIndex + 3]],
            vertices[tris[quad.trisIndex + 4]],
            vertices[tris[quad.trisIndex + 5]]);
        if (t1 > 0.0f && t2 > 0.0f) {
            return t1 < t2 ? t1 : t2;
        } else {
            return t1 > t2 ? t1 : t2;
        }
    }

    private float GetHeightAtMouse() {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 midpoint = lastSelected.pos + new Vector3(0.5f, 0.0f, 0.5f);
        Plane plane = new Plane(-1.0f * Camera.current.transform.forward, midpoint);
        plane.Raycast(ray, out float enter);

        Vector3 hit = ray.GetPoint(enter);
        float height = Mathf.Round(hit.y * 2.0f) / 2.0f;
        return height > 0 ? height : 0;
    }
}
