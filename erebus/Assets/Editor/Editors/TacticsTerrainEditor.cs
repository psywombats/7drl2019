using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
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
        PaletteTileDrag,
        RlickDrag,
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
    private Vector2 selectionSize = Vector2Int.zero;

    private EditMode mode = EditMode.None;
    private SelectionTool tool = SelectionTool.Select;

    private GridPalette palette;
    private Tilemap tileset;
    private Vector2 selectedTileStart;
    private Rect tileSelectRect;
    private Tile[] paletteBuffer;
    private Vector2Int paletteBufferSize = new Vector2Int(0, 0);
    private bool wraparoundPaintMode;

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
        Vector2Int flooredSelection = new Vector2Int(Mathf.FloorToInt(selectionSize.x), Mathf.FloorToInt(selectionSize.y));
        Vector2Int newSelectionSize = EditorGUILayout.Vector2IntField("Brush size", flooredSelection);
        if (newSelectionSize != flooredSelection) {
            selectionSize = new Vector2(Mathf.Max(1.0f, newSelectionSize.x), Mathf.Max(1.0f, newSelectionSize.y));
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

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        EventType typeForControl = Event.current.GetTypeForControl(controlId);
        Vector2 mousePos = Event.current.mousePosition;

        GUIStyle style = new GUIStyle();
        style.padding = new RectOffset(0, 0, 0, 0);

        if (tileset != null && tool == SelectionTool.Paint) {
            wraparoundPaintMode = EditorGUILayout.Toggle("Paint all faces", wraparoundPaintMode);
            Texture2D backer = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/White.png");
            for (int y = tileset.size.y - 1; y >= 0; y -= 1) {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < tileset.size.x; x += 1) {
                    Rect selectRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(Map.TileSizePx), GUILayout.Height(Map.TileSizePx));
                    Tile tile = tileset.GetTile<Tile>(new Vector3Int(x, y, 0));

                    GUILayout.Box("", style, GUILayout.Width(Map.TileSizePx), GUILayout.Height(Map.TileSizePx));
                    Rect r = GUILayoutUtility.GetLastRect();

                    if (r.Contains(Event.current.mousePosition)) {
                        switch (Event.current.type) {
                            case EventType.MouseDown:
                                mode = EditMode.PaletteTileDrag;
                                selectedTileStart = new Vector2(x, y);
                                tileSelectRect = new Rect(x, y, 1, 1);
                                paletteBufferSize = Vector2Int.zero;
                                break;
                            case EventType.MouseDrag:
                                if (mode == EditMode.PaletteTileDrag) {
                                    int minX = (int)Mathf.Min(selectedTileStart.x, x);
                                    int minY = (int)Mathf.Min(selectedTileStart.y, y);
                                    tileSelectRect = new Rect(minX, minY,
                                            (int)Mathf.Max(selectedTileStart.x, x) + 1 - minX,
                                            (int)Mathf.Max(selectedTileStart.y, y) + 1 - minY);
                                    selectionSize = new Vector2(tileSelectRect.width, tileSelectRect.height);
                                }
                                break;
                            case EventType.MouseUp:
                                mode = EditMode.None;
                                break;
                        }
                    }

                    Rect rect = new Rect(tile.sprite.uv[0].x, tile.sprite.uv[3].y,
                        tile.sprite.uv[3].x - tile.sprite.uv[0].x,
                        tile.sprite.uv[0].y - tile.sprite.uv[3].y);

                    GUI.DrawTextureWithTexCoords(r, tile.sprite.texture, rect, true);
                    if (r.Contains(Event.current.mousePosition)) {
                        GUI.DrawTexture(r, backer, ScaleMode.StretchToFill, true, 0.0f, new Color(1, 0, 0, 0.5f), 0.0f, 0.0f);
                    } else if (paletteBufferSize == Vector2Int.zero && tileSelectRect.Contains(new Vector2(x, y))) {
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
        EventType typeForControl = Event.current.GetTypeForControl(controlId);
        switch (Event.current.button) {
            case 0:
                HandleLeftclick(typeForControl, controlId);
                break;
            case 1:
                HandleRightclick(typeForControl, controlId);
                break;
        }

        TerrainQuad quad = GetSelectedQuad();
        switch (typeForControl) {
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

    private void HandleLeftclick(EventType typeForControl, int controlId) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        switch (typeForControl) {
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
                        primarySelection = GetSelectedQuad();
                        CaptureSelection(primarySelection);
                        PaintTileIfNeeded();
                        break;
                }
                break;
            case EventType.MouseUp:
                switch (mode) {
                    case EditMode.AdjustingHeight:
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
                        mode = EditMode.None;
                        break;
                }
                break;
            case EventType.ScrollWheel:
                if (mode == EditMode.None && selectedQuads.Count > 0) {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    float maxSelection = Mathf.Max(selectionSize.x, selectionSize.y);
                    maxSelection += -1.0f * Event.current.delta.y / 5.0f;
                    selectionSize = new Vector2(maxSelection, maxSelection);
                    if (Mathf.RoundToInt(selectionSize.x) < 1) selectionSize.x = 1;
                    if (Mathf.RoundToInt(selectionSize.y) < 1) selectionSize.y = 1;
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

    private void HandleRightclick(EventType typeForControl, int controlId) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        switch (typeForControl) {
            case EventType.MouseDown:
                Debug.Log("down");
                if (tool != SelectionTool.Select) {
                    TerrainQuad quad = GetSelectedQuad();
                    if (quad != null) {
                        ConsumeEvent(controlId);
                        primarySelection = quad;
                        selectionSize = new Vector2(1.0f, 1.0f);
                        CaptureSelection(quad);
                        tool = SelectionTool.Paint;
                        mode = EditMode.RlickDrag;
                    }
                }
                break;
            case EventType.MouseDrag:
                if (mode == EditMode.RlickDrag) {
                    ConsumeEvent(controlId);
                    TerrainQuad quad = GetSelectedQuad();
                    if (quad != null && quad.normal == primarySelection.normal) {
                        selectedQuads = MathHelper3D.GetQuadsInRect(quads, quad, primarySelection);
                    }
                }
                break;
            case EventType.MouseUp:
                Debug.Log("up");
                switch (mode) {
                    case EditMode.Selected:
                        mode = EditMode.None;
                        break;
                    case EditMode.RlickDrag:
                        mode = EditMode.None;
                        TerrainQuad quad = GetSelectedQuad();
                        if (quad != null && quad.normal == primarySelection.normal) {
                            selectedQuads = MathHelper3D.GetQuadsInRect(quads, quad, primarySelection);
                            Vector3 v1 = quad.pos;
                            Vector3 v2 = primarySelection.pos;
                            float x1 = Mathf.Min(v1.x, v2.x);
                            float x2 = Mathf.Max(v1.x, v2.x);
                            float y1 = Mathf.Min(v1.y, v2.y);
                            float y2 = Mathf.Max(v1.y, v2.y);
                            float z1 = Mathf.Min(v1.z, v2.z);
                            float z2 = Mathf.Max(v1.z, v2.z);
                            if (z1 != z2 && y1 != y2) {
                                paletteBufferSize = new Vector2Int((int)(z2 - z1 + 1), (int)((y2 - y1 + 0.5f) * 2.0f));
                                paletteBuffer = new Tile[paletteBufferSize.x * paletteBufferSize.y];
                                for (float z = z1; z <= z2; z += 1.0f) {
                                    for (float y = y1; y <= y2; y += 0.5f) {
                                        TerrainQuad at = quads[new Vector3(v1.x, y, z)][quad.normal];
                                        paletteBuffer[(int)(paletteBufferSize.x * (2.0f * (y - y1)) + (z - z1))] = at.tile;
                                    }
                                }
                            } else if (x1 != x2 && y1 != y2) {
                                paletteBufferSize = new Vector2Int((int)(x2 - x1 + 1), (int)((y2 - y1 + 0.5f) * 2.0f));
                                paletteBuffer = new Tile[paletteBufferSize.x * paletteBufferSize.y];
                                for (float x = x1; x <= x2; x += 1.0f) {
                                    for (float y = y1; y <= y2; y += 0.5f) {
                                        TerrainQuad at = quads[new Vector3(x, y, v1.z)][quad.normal];
                                        paletteBuffer[(int)(paletteBufferSize.x * (2.0f * (y - y1)) + (x - x1))] = at.tile;
                                    }
                                }
                            } else {
                                paletteBufferSize = new Vector2Int((int)(x2 - x1 + 1), (int)(z2 - z1 + 1));
                                paletteBuffer = new Tile[paletteBufferSize.x * paletteBufferSize.y];
                                for (float x = x1; x <= x2; x += 1.0f) {
                                    for (float z = z1; z <= z2; z += 1.0f) {
                                        TerrainQuad at = quads[new Vector3(x, v1.y, z)][quad.normal];
                                        paletteBuffer[(int)(paletteBufferSize.x * (z - z1) + (x - x1))] = at.tile;
                                    }
                                }
                            }
                            selectionSize = paletteBufferSize;
                        }
                        break;
                }
                break;
        }
    }

    private void Rebuild(bool regenMesh) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(terrain.gameObject);
        if (prefabStage != null) {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }

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

    private void RepaintMesh() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;

        Vector2[] uvArray = new Vector2[terrain.GetComponent<MeshFilter>().sharedMesh.uv.Length];
        foreach (Dictionary<Vector3, TerrainQuad> quadDictionary in quads.Values) {
            foreach (TerrainQuad quad in quadDictionary.Values) {
                quad.CopyUVs(uvArray);
            }
        }

        terrain.GetComponent<MeshFilter>().sharedMesh.uv = uvArray;
        uvs = new List<Vector2>(uvArray);
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
        tileSelectRect = new Rect(0, 0, 1, 1);
        paletteBufferSize = Vector2Int.zero;
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        palette = newPalette;
        terrain.paletteName = palette.name;
        string palettePath = "Assets/Tilesets/Palettes/" + palette.name + ".prefab";
        GameObject tilesetObject = AssetDatabase.LoadAssetAtPath<GameObject>(palettePath);
        tileset = tilesetObject.transform.GetChild(0).GetComponent<Tilemap>();
    }

    private void PaintTileIfNeeded() {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        foreach (TerrainQuad quad in selectedQuads) {
            if (quad.normal.y > 0.0f) {
                int originX = (int)primarySelection.pos.x - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.x) / 2.0f);
                int originY = (int)primarySelection.pos.z - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.y) / 2.0f);
                Tile tile = TileForSelection((int)(quad.pos.x - originX), (int)(quad.pos.z - originY));
                UpdateTile(quad, tile);
            } else {
                Tile tile;
                if (quad.normal.x != 0.0f) {
                    int originX = (int)primarySelection.pos.z - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.x) / 2.0f);
                    int originY = (int)primarySelection.pos.y - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.y) / 2.0f);
                    tile = TileForSelection((int)(quad.pos.z - originX), (int)(quad.pos.y - originY));
                } else {
                    int originX = (int)primarySelection.pos.x - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.x) / 2.0f);
                    int originY = (int)primarySelection.pos.y - Mathf.FloorToInt(Mathf.RoundToInt(selectionSize.y) / 2.0f);
                    tile = TileForSelection((int)(quad.pos.x - originX), (int)(quad.pos.y - originY));
                }
                if (wraparoundPaintMode) {
                    foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
                        UpdateTile(quad, dir, tile);
                    }
                } else {
                    UpdateTile(quad, OrthoDirExtensions.DirectionOf3D(quad.normal), tile);
                }
            }
        }
        RepaintMesh();
        primarySelection = GetSelectedQuad();
        CaptureSelection(primarySelection);
    }

    private void CaptureSelection(TerrainQuad quad) {
        selectedQuads = MathHelper3D.GetQuadsAroundQuad(quads, quad, selectionSize);
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

    private void UpdateTile(TerrainQuad quad, Tile tile) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        quad.UpdateTile(tile, tileset, 0.0f);
        terrain.SetTile((int)quad.pos.x, (int)quad.pos.z, tile);
    }

    private void UpdateTile(TerrainQuad quad, OrthoDir dir, Tile tile) {
        TacticsTerrainMesh terrain = (TacticsTerrainMesh)target;
        quad.UpdateTile(tile, tileset, quad.pos.y);
        terrain.SetTile((int)quad.pos.x, (int)quad.pos.z, quad.pos.y - 0.5f, dir, tile);
    }

    private Tile TileForSelection(int x, int y) {
        if (paletteBufferSize == Vector2.zero) {
            return tileset.GetTile<Tile>(new Vector3Int(
                (int)tileSelectRect.x + (x % (int)tileSelectRect.width),
                (int)tileSelectRect.y + (y % (int)tileSelectRect.height),
                0));
        } else {
            return paletteBuffer[
                (x % paletteBufferSize.x) +
                (y % paletteBufferSize.y) * (paletteBufferSize.x)];
        }

    }
}
