using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TacticsTerrainMesh))]
public class TacticsTerrainEditor : Editor {
    
    private enum EditMode {
        None,
        HeightAdjust,
    }

    // tortured data structure
    // index by pos -> normal ->
    private Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>> quads;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> tris;

    private List<TerrainQuad> selectedQuads = new List<TerrainQuad>();
    private TerrainQuad primarySelection;
    private float selectedHeight;
    private float selectionSize = 1;

    private EditMode mode = EditMode.None;

    private GridPalette palette;
    private Tilemap tileset;
    private Tile selectedTile;

    public override bool RequiresConstantRepaint() {
        return true;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        GUILayout.Space(20.0f);
        if (GUILayout.Button("Rebuild")) {
            Rebuild(true);
        }
        Vector2Int newSize = EditorGUILayout.Vector2IntField("Size", terrain.size);
        if (newSize != terrain.size) {
            terrain.Resize(newSize);
            Rebuild(true);
        }
        GUILayout.Space(20.0f);
        int newSelectionSize = EditorGUILayout.IntField("Brush size", Mathf.FloorToInt(selectionSize));
        if (newSelectionSize != Mathf.FloorToInt(selectionSize)) {
            selectionSize = newSelectionSize > 0 ? newSelectionSize : 1;
        }

        if (palette == null) {
            if (terrain.paletteName != null && terrain.paletteName.Length > 0) {
                string paletteName = "Assets/Tilesets/Palettes/" + terrain.paletteName + ".prefab";
                UpdateWithPalette(AssetDatabase.LoadAssetAtPath<GridPalette>(paletteName));
            }
        }
        GridPalette newPalette = (GridPalette)EditorGUILayout.ObjectField("Tileset", palette, typeof(GridPalette), false);
        if (newPalette != palette) {
            UpdateWithPalette(newPalette);
        }

        if (tileset != null) {
            Texture2D backer = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/White.png");
            for (int y = tileset.size.y - 1; y >= 0; y -= 1) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUI.backgroundColor = new Color(0.5f, 0.5f, 1.0f);

                for (int x = 0; x < tileset.size.x; x += 1) {
                    Rect selectRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(Map.TileSizePx), GUILayout.Height(Map.TileSizePx));
                    
                    if (GUILayout.Button("", GUILayout.Width(Map.TileSizePx), GUILayout.Width(Map.TileSizePx))) {
                        Tile newSelect = tileset.GetTile<Tile>(new Vector3Int(x, y, 0));
                        if (newSelect == selectedTile) {
                            selectedTile = null;
                        } else {
                            selectedTile = newSelect;
                        }
                    }

                    Rect r = GUILayoutUtility.GetLastRect();
                    
                    Tile tile = tileset.GetTile<Tile>(new Vector3Int(x, y, 0));
                    Rect rect = new Rect(tile.sprite.uv[0].x, tile.sprite.uv[3].y,
                        tile.sprite.uv[3].x - tile.sprite.uv[0].x,
                        tile.sprite.uv[0].y - tile.sprite.uv[3].y);

                    Rect expanded = new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4);
                    if (r.Contains(Event.current.mousePosition)) {
                        GUI.DrawTexture(expanded, backer, ScaleMode.ScaleToFit, true, 0.0f, Color.red, 0.0f, 0.0f);
                    } else if (tileset.GetTile<Tile>(new Vector3Int(x, y, 0)) == selectedTile) {
                        GUI.DrawTexture(expanded, backer, ScaleMode.ScaleToFit, true, 0.0f, Color.blue, 0.0f, 0.0f);
                    }
                    GUI.DrawTextureWithTexCoords(r, tile.sprite.texture, rect, true);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public void OnSceneGUI() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        bool dirty = false;
        if (quads == null) {
            Rebuild(false);
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.MouseMove:
                switch (mode) {
                    case EditMode.None:
                        TerrainQuad quad = GetSelectedQuad();
                        if (quad != primarySelection) {
                            CaptureSelection(quad);
                            primarySelection = quad;
                            SceneView.RepaintAll();
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
                    case EditMode.None:
                        PaintTileIfNeeded();
                        if (selectedTile != null) {
                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                        }
                        break;
                }
                break;
            case EventType.MouseDown:
                CaptureSelection(GetSelectedQuad());
                PaintTileIfNeeded();
                if (mode == EditMode.None) {
                    if (selectedQuads.Count > 0 && selectedQuads[0].normal.y > 0.0f) {
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
                        foreach (TerrainQuad quad in selectedQuads) {
                            int x = Mathf.RoundToInt(quad.pos.x);
                            int y = Mathf.RoundToInt(quad.pos.z);
                            if (terrain.HeightAt(x, y) != height) {
                                terrain.SetHeight(x, y, height);
                                dirty = true;
                            }
                        }

                        break;
                }
                break;
            case EventType.ScrollWheel:
                if (mode == EditMode.None && selectedQuads.Count > 0) {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    selectionSize += -1.0f * Event.current.delta.y / 10.0f;
                    selectionSize = selectionSize < 1.0f ? 1.0f : selectionSize;
                    CaptureSelection(primarySelection);
                    SceneView.RepaintAll();
                }
                break;
        }

        if (selectedHeight > 0.0f && mode == EditMode.HeightAdjust) {
            int x = Mathf.RoundToInt(primarySelection.pos.x);
            int y = Mathf.RoundToInt(primarySelection.pos.z);
            float h = terrain.HeightAt(x, y);
            foreach (TerrainQuad quad in selectedQuads) {
                for (float z = GetHeightAtMouse(); Mathf.Abs(z - h) > 0.1f; z += 0.5f * Mathf.Sign(h - z)) {
                    Handles.DrawWireCube(new Vector3(quad.pos.x + 0.5f, z + 0.25f * Mathf.Sign(h - z), quad.pos.z + 0.5f),
                        new Vector3(1.0f, 0.5f, 1.0f));
                }
            }
        }
        if (mode == EditMode.None || (mode == EditMode.HeightAdjust)) {
            DrawSelection();
        }
        if (dirty) {
            Rebuild(true);
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

        quads = new Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>>();
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
                                terrain.TileAt(x, z, y, dir),
                                new Vector3(x, y + 0.5f, z), dir.Px3D());
                        }
                    }
                }
            }
        }

        if (regenMesh) {
            selectedQuads.Clear();
            mesh.Clear();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateBounds();
        }
    }

    private void AddQuad(Vector3 lowerLeft, Vector3 upperRight, Tile tile, Vector3 pos, Vector3 normal) {
        TerrainQuad quad = new TerrainQuad(tris.Count, vertices.Count, pos, normal);
        if (!quads.ContainsKey(pos)) {
            quads[pos] = new Dictionary<Vector3, TerrainQuad>();
        }
        quads[pos][normal] = quad;

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
            spriteUVs = AdjustZ(spriteUVs, lowerLeft.y, normal.x == 0.0f);
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

    private TerrainQuad GetSelectedQuad() {
        if (quads == null) {
            return null;
        }

        Vector2 mousePos = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        float bestT = -1.0f;
        TerrainQuad best = null;
        foreach (Dictionary<Vector3, TerrainQuad> quadDictionary in quads.Values) {
            foreach (TerrainQuad quad in quadDictionary.Values) {
                float t = RayDistanceForQuad(ray, quad);
                if (t > 0.0f && (t < bestT || bestT == -1.0f)) {
                    bestT = t;
                    best = quad;
                }
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
        Vector3 midpoint = primarySelection.pos + new Vector3(0.5f, 0.0f, 0.5f);
        Plane plane = new Plane(-1.0f * Camera.current.transform.forward, midpoint);
        plane.Raycast(ray, out float enter);

        Vector3 hit = ray.GetPoint(enter);
        float height = Mathf.Round(hit.y * 2.0f) / 2.0f;
        return height > 0 ? height : 0;
    }

    private void UpdateWithPalette(GridPalette newPalette) {
        selectedTile = null;

        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        palette = newPalette;
        terrain.paletteName = palette.name;
        string palettePath = "Assets/Tilesets/Palettes/" + palette.name + ".prefab";
        GameObject tilesetObject = AssetDatabase.LoadAssetAtPath<GameObject>(palettePath);
        tileset = tilesetObject.transform.GetChild(0).GetComponent<Tilemap>();
    }

    private void PaintTileIfNeeded() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        if (selectedTile != null) {
            foreach (TerrainQuad quad in selectedQuads) {
                int x = Mathf.RoundToInt(quad.pos.x);
                int y = Mathf.RoundToInt(quad.pos.z);
                if (quad.normal.y > 0.0f) {
                    terrain.SetTile(x, y, selectedTile);
                } else {
                    float height = quad.pos.y - 0.5f;
                    terrain.SetTile(x, y, height, DirForNormal(quad.normal), selectedTile);
                }
            }
            Rebuild(true);
            primarySelection = GetSelectedQuad();
            CaptureSelection(primarySelection);
        }
    }

    private OrthoDir DirForNormal(Vector3 normal) {
        Vector3 normalized = normal.normalized;
        foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
            if (normalized == dir.Px3D()) {
                return dir;
            }
        }
        Debug.Assert(false, "bad normal " + normal);
        return OrthoDir.South;
    }

    // given some uvs, split them up based on our height
    // because y can sometimes be split across tiles
    // xMode is a hack to just get stuff looking right
    private Vector2[] AdjustZ(Vector2[] origUVs, float lowerHeight, bool xMode) {
        if (tileset == null) {
            return origUVs;
        }
        float unit = 1.0f / tileset.size.y;
        if (xMode) {
            if (Math.Abs(Mathf.Round(lowerHeight) - lowerHeight) > 0.1f) {
                return new Vector2[] {
                    origUVs[0],
                    origUVs[1],
                    new Vector2(origUVs[2].x, origUVs[2].y + unit / 2.0f),
                    new Vector2(origUVs[3].x, origUVs[3].y + unit / 2.0f),
                };
            } else {
                return new Vector2[] {
                    new Vector2(origUVs[0].x, origUVs[0].y - unit / 2.0f),
                    new Vector2(origUVs[1].x, origUVs[1].y - unit / 2.0f),
                    origUVs[2],
                    origUVs[3],
                };
            }
        } else {
            if (Math.Abs(Mathf.Round(lowerHeight) - lowerHeight) < 0.1f) {
                return new Vector2[] {
                    new Vector2(origUVs[2].x, origUVs[2].y),
                    new Vector2(origUVs[0].x, origUVs[0].y - unit / 2.0f),
                    new Vector2(origUVs[3].x, origUVs[3].y),
                    new Vector2(origUVs[1].x, origUVs[1].y - unit / 2.0f),
                };
            } else {
                return new Vector2[] {
                    new Vector2(origUVs[2].x, origUVs[2].y + unit / 2.0f),
                    new Vector2(origUVs[0].x, origUVs[0].y),
                    new Vector2(origUVs[3].x, origUVs[3].y + unit / 2.0f),
                    new Vector2(origUVs[1].x, origUVs[1].y),
                };
            }
        }
    }

    private void CaptureSelection(TerrainQuad quad) {
        selectedQuads.Clear();
        if (quad == null) {
            return;
        }
        int d = Mathf.RoundToInt(selectionSize);
        int low = -1 * Mathf.CeilToInt(d / 2.0f) + 1;
        int high = Mathf.FloorToInt(d / 2.0f);
        for (int i = low; i <= high; i += 1) {
            for (int j = low; j <= high; j += 1) {
                Vector3 newPos = quad.pos;
                if (quad.normal.x != 0) {
                    newPos = new Vector3(
                        quad.pos.x,
                        quad.pos.y + i * 0.5f,
                        quad.pos.z + j);
                } else if (quad.normal.y != 0) {
                    newPos = new Vector3(
                        quad.pos.x + i,
                        quad.pos.y,
                        quad.pos.z + j);
                } else if (quad.normal.z != 0) {
                    newPos = new Vector3(
                        quad.pos.x + i,
                        quad.pos.y + j * 0.5f,
                        quad.pos.z);
                }
                if (quads.ContainsKey(newPos) && quads[newPos].ContainsKey(quad.normal)) {
                    selectedQuads.Add(quads[newPos][quad.normal]);
                }
            }
        }
    }

    private void DrawSelection() {
        foreach (TerrainQuad quad in selectedQuads) {
            Vector3 mid = quad.pos + new Vector3(0.5f, -0.25f, 0.5f) + new Vector3(
                quad.normal.x * 0.5f,
                quad.normal.y * 0.25f,
                quad.normal.z * 0.5f);
            Vector3 size = new Vector3(
                1.01f - Mathf.Abs(quad.normal.x),
                0.51f - (Mathf.Abs(quad.normal.y) * 0.5f),
                1.01f - Mathf.Abs(quad.normal.z));
            Handles.color = new Color(1.0f, 1.0f, 1.0f);
            Handles.DrawWireCube(mid, size);
        }
    }
}
