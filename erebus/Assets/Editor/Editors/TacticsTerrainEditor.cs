using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[CustomEditor(typeof(TacticsTerrainMesh))]
public class TacticsTerrainEditor : Editor {

    private static readonly string GenericPrefabPath = "Assets/Resources/Prefabs/MapEvent3D.prefab";
    private static readonly string TacticsPrefabPath = "Assets/Resources/Prefabs/Tactics/TacticsDollEvent.prefab";

    private enum EditMode {
        None,
        AdjustingHeight,
        Painting,
        Selected,
    }

    private enum SelectionTool {
        Select,
        Paint,
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
    private Vector3 paintingNormal;
    private float selectedHeight;
    private float selectionSize = 1;

    private EditMode mode = EditMode.None;
    private SelectionTool tool = SelectionTool.Select;

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

        SelectionTool[] ordinals = new SelectionTool[] {
            SelectionTool.Select, SelectionTool.Paint, SelectionTool.HeightAdjust
        };
        string[] names = new string[] { "Select", "Paint", "HeightAdjust" };
        int selectionIndex = GUILayout.SelectionGrid(ArrayUtility.IndexOf(ordinals, tool), names, names.Length);
        tool = ordinals[selectionIndex];

        if (tileset != null && tool == SelectionTool.Paint) {
            Texture2D backer = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/White.png");
            for (int y = tileset.size.y - 1; y >= 0; y -= 1) {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < tileset.size.x; x += 1) {
                    Rect selectRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(Map.TileSizePx), GUILayout.Height(Map.TileSizePx));

                    GUIStyle style = new GUIStyle();
                    style.padding = new RectOffset(0, 0, 0, 0);
                    if (GUILayout.Button("", style, GUILayout.Width(Map.TileSizePx), GUILayout.Height(Map.TileSizePx))) {
                        Tile newSelect = tileset.GetTile<Tile>(new Vector3Int(x, y, 0));
                        if (newSelect == selectedTile) {
                            selectedTile = null;
                        } else {
                            selectedTile = newSelect;
                            tool = SelectionTool.Paint;
                        }
                    }

                    Rect r = GUILayoutUtility.GetLastRect();
                    
                    Tile tile = tileset.GetTile<Tile>(new Vector3Int(x, y, 0));
                    Rect rect = new Rect(tile.sprite.uv[0].x, tile.sprite.uv[3].y,
                        tile.sprite.uv[3].x - tile.sprite.uv[0].x,
                        tile.sprite.uv[0].y - tile.sprite.uv[3].y);
                    
                    GUI.DrawTextureWithTexCoords(r, tile.sprite.texture, rect, true);
                    if (r.Contains(Event.current.mousePosition)) {
                        GUI.DrawTexture(r, backer, ScaleMode.StretchToFill, true, 0.0f, new Color(1, 0, 0, 0.5f), 0.0f, 0.0f);
                    } else if (tileset.GetTile<Tile>(new Vector3Int(x, y, 0)) == selectedTile) {
                        GUI.DrawTexture(r, backer, ScaleMode.StretchToFill, true, 0.0f, new Color(1, 1, 1, 0.8f), 0.0f, 0.0f);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if (tool == SelectionTool.Select) {
            EditorGUI.BeginDisabledGroup(mode != EditMode.Selected);
            if (GUILayout.Button("Create MapEvent3D")) {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GenericPrefabPath);
                MapEvent3D mapEvent = Instantiate(prefab).GetComponent<MapEvent3D>();
                mapEvent.name = "Event" + Random.Range(1000000, 9999999);
                AddEvent(mapEvent);
            }
            if (GUILayout.Button("Create Tactics Doll")) {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TacticsPrefabPath);
                MapEvent3D mapEvent = Instantiate(prefab).GetComponent<MapEvent3D>();
                mapEvent.name = "Doll" + Random.Range(1000000, 9999999);
                AddEvent(mapEvent);
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    public void OnSceneGUI() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        if (quads == null) {
            Rebuild(false);
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        switch (Event.current.button) {
            case 0:
                HandleLeftclick(controlId);
                break;
            case 1:
                HandleRightclick(controlId);
                break;
        }

        TerrainQuad quad = GetSelectedQuad();
        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.MouseMove:
                switch (mode) {
                    case EditMode.None:
                    case EditMode.Painting:
                        if (quad != primarySelection) {
                            CaptureSelection(quad);
                            primarySelection = quad;
                            SceneView.RepaintAll();
                        }
                        break;
                }
                break;
        }

        MathHelper3D.DrawQuads(selectedQuads, Color.white);
    }

    private void HandleLeftclick(int controlId) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.MouseDown:
                switch (tool) {
                    case SelectionTool.HeightAdjust:
                        if (selectedQuads.Count > 0 && selectedQuads[0].normal.y > 0.0f) {
                            ConsumeEvent(controlId);
                            mode = EditMode.AdjustingHeight;
                            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                            selectedHeight = 0.0f;
                        }
                        break;
                    case SelectionTool.Paint:
                        if (selectedQuads.Count > 0) {
                            ConsumeEvent(controlId);
                            PaintTileIfNeeded();
                            mode = EditMode.Painting;
                            paintingNormal = primarySelection.normal;
                        }
                        break;
                    case SelectionTool.Select:
                        ConsumeEvent(controlId);
                        if (mode == EditMode.Selected) {
                            TerrainQuad newQuad = GetSelectedQuad();
                            if (newQuad == null || newQuad == primarySelection) {
                                primarySelection = null;
                            } else {
                                primarySelection = newQuad;
                                CaptureSelection(primarySelection);
                            }
                        }
                        mode = primarySelection == null ? EditMode.None : EditMode.Selected;
                        break;
                }
                break;
            case EventType.MouseDrag:
                switch (mode) {
                    case EditMode.AdjustingHeight:
                        ConsumeEvent(controlId);
                        selectedHeight = MathHelper3D.GetHeightAtMouse(primarySelection);
                        break;
                    case EditMode.Painting:
                        ConsumeEvent(controlId);
                        PaintTileIfNeeded();
                        break;
                }
                break;
            case EventType.MouseUp:
                switch (mode) {
                    case EditMode.AdjustingHeight:
                        ConsumeEvent(controlId);
                        mode = EditMode.None;
                        bool dirty = false;

                        float height = MathHelper3D.GetHeightAtMouse(primarySelection);
                        foreach (TerrainQuad quad in selectedQuads) {
                            int x = Mathf.RoundToInt(quad.pos.x);
                            int y = Mathf.RoundToInt(quad.pos.z);
                            if (terrain.HeightAt(x, y) != height) {
                                terrain.SetHeight(x, y, height);
                                dirty = true;
                            }
                        }
                        if (dirty) {
                            Rebuild(true);
                        }
                        break;
                    case EditMode.Painting:
                        ConsumeEvent(controlId);
                        mode = EditMode.None;
                        break;
                }
                break;
            case EventType.ScrollWheel:
                if (mode == EditMode.None && selectedQuads.Count > 0) {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    selectionSize += -1.0f * Event.current.delta.y / 5.0f;
                    selectionSize = selectionSize < 1.0f ? 1.0f : selectionSize;
                    CaptureSelection(primarySelection);
                    SceneView.RepaintAll();
                }
                break;
        }

        if (selectedHeight >= 0.0f && mode == EditMode.AdjustingHeight) {
            int x = Mathf.RoundToInt(primarySelection.pos.x);
            int y = Mathf.RoundToInt(primarySelection.pos.z);
            float h = terrain.HeightAt(x, y);
            foreach (TerrainQuad quad in selectedQuads) {
                float z = MathHelper3D.GetHeightAtMouse(primarySelection);
                for (; Mathf.Abs(z - h) > 0.1f; z += 0.5f * Mathf.Sign(h - z)) {
                    Handles.DrawWireCube(new Vector3(quad.pos.x + 0.5f, z + 0.25f * Mathf.Sign(h - z), quad.pos.z + 0.5f),
                        new Vector3(1.0f, 0.5f, 1.0f));
                }
            }
        }
    }

    private void HandleRightclick(int controlId) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.MouseUp:
                switch (tool) {
                    case SelectionTool.Select:
                        mode = EditMode.None;
                        break;
                    case SelectionTool.Paint:
                        if (primarySelection != null) {
                            tool = SelectionTool.Paint;
                            if (primarySelection.normal.y == 0.0f) {
                                selectedTile = terrain.TileAt(
                                    (int)primarySelection.pos.x,
                                    (int)primarySelection.pos.z,
                                    primarySelection.pos.y,
                                    OrthoDirExtensions.DirectionOf(primarySelection.normal));
                            } else {
                                selectedTile = terrain.TileAt((int)primarySelection.pos.x, (int)primarySelection.pos.z);
                            }
                        }
                        break;
                }
                break;
        }
    }

    private void Rebuild(bool regenMesh) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        MeshFilter filter = terrain.GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            AssetDatabase.CreateAsset(mesh, "Assets/Resources/TacticsMaps/Meshes/" + terrain.gameObject.name + ".asset");
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
            mesh.RecalculateNormals();
        }
    }

    private void AddQuad(Vector3 lowerLeft, Vector3 upperRight, Tile tile, Vector3 pos, Vector3 normal) {
        TerrainQuad quad = new TerrainQuad(tris, vertices, uvs, lowerLeft, upperRight, tile, tileset, pos, normal);
        if (!quads.ContainsKey(pos)) {
            quads[pos] = new Dictionary<Vector3, TerrainQuad>();
        }
        quads[pos][normal] = quad;
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
                if (mode == EditMode.Painting && paintingNormal != quad.normal) {
                    continue;
                }
                float t = MathHelper3D.RayDistanceForQuad(vertices, tris, ray, quad);
                if (t > 0.0f && (t < bestT || bestT == -1.0f)) {
                    bestT = t;
                    best = quad;
                }
            }
        }

        return best;
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
                    terrain.SetTile(x, y, height, OrthoDirExtensions.DirectionOfPx(quad.normal), selectedTile);
                }
            }
            Rebuild(true);
            primarySelection = GetSelectedQuad();
            CaptureSelection(primarySelection);
        }
    }

    private void CaptureSelection(TerrainQuad quad) {
        selectedQuads = MathHelper3D.GetQuadsInGrid(quads, quad, selectionSize);
    }

    private void ConsumeEvent(int controlId) {
        GUIUtility.hotControl = controlId;
        Event.current.Use();
    }

    private void AddEvent(MapEvent3D mapEvent) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        Map map = terrain.GetComponent<Map>();
        GameObjectUtility.SetParentAndAlign(mapEvent.gameObject, map.objectLayer.gameObject);
        Undo.RegisterCreatedObjectUndo(mapEvent, "Create " + mapEvent.name);
        mapEvent.SetLocation(new Vector2Int((int)primarySelection.pos.x, (int)primarySelection.pos.z));
        Selection.activeObject = mapEvent.gameObject;
    }
}
