using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(Map))]
public class TacticsTerrainMesh : MonoBehaviour, ISerializationCallbackReceiver {
    
    [HideInInspector]
    public Vector2Int size;
    [HideInInspector]
    public float[] heights;
    [HideInInspector]
    public string paletteName;
    [HideInInspector]
    public Tilemap tileset;

    public Tile defaultTopTile;
    public Tile defaultFaceTile;

    [HideInInspector]
    public FacingTileDictionary serializedFacingTiles;
    private Dictionary<FacingTileKey, Tile> facingTiles = new Dictionary<FacingTileKey, Tile>();

    private Tilemap _tilemap;
    public Tilemap tilemap {
        get {
            if (_tilemap == null) _tilemap = GetComponent<Tilemap>();
            return _tilemap;
        }
    }

    // === CONSTRUCTION  ===========================================================================

    public void ClearTiles() {
        facingTiles.Clear();
        heights = new float[size.x * size.y];
    }

    public void Resize(Vector2Int newSize) {
        List<FacingTileKey> toRemove = new List<FacingTileKey>();
        foreach (FacingTileKey key in facingTiles.Keys) {
            if (key.pos.x >= newSize.x || key.pos.z >= newSize.y) {
                toRemove.Add(key);
            }
        }
        foreach (FacingTileKey key in toRemove) {
            facingTiles.Remove(key);
        }

        float[] newHeights = new float[newSize.x * newSize.y];
        Tile[] newTiles = new Tile[newSize.x * newSize.y];
        for (int y = 0; y < (size.y < newSize.y ? size.y : newSize.y); y += 1) {
            for (int x = 0; x < (size.x < newSize.x ? size.x : newSize.x); x += 1) {
                newHeights[y * newSize.x + x] = heights[y * size.x + x];
                newTiles[y * newSize.x + x] = tilemap.GetTile<Tile>(new Vector3Int(x, y, 0));
            }
        }
        for (int y = size.y; y < newSize.y; y += 1) {
            for (int x = size.x; x < newSize.x; x += 1) {
                newHeights[y * size.x + x] = 0.5f;
            }
        }

        tilemap.ClearAllTiles();
        for (int y = 0; y < (size.y < newSize.y ? size.y : newSize.y); y += 1) {
            for (int x = 0; x < (size.x < newSize.x ? size.x : newSize.x); x += 1) {
                tilemap.SetTile(new Vector3Int(x, y, 0), newTiles[y * (size.x < newSize.x ? size.x : newSize.x) + x]);
            }
        }
        heights = newHeights;
        size = newSize;
    }

    // === MESH GENERATION =========================================================================

    public MeshGenerationResult Rebuild(bool regenMesh) {
        MeshFilter filter = GetComponent<MeshFilter>();
        
        if (!Application.isPlaying) {
            if (filter.sharedMesh == null) {
                filter.sharedMesh = new Mesh();
                AssetDatabase.CreateAsset(filter.sharedMesh,
                    "Assets/Resources/TacticsMaps/Meshes/" + gameObject.name + ".asset");
            }
        } else {
            if (filter.mesh == null) {
                filter.mesh = new Mesh();
            }
        }

        Mesh mesh;
        if (!Application.isPlaying) {
            mesh = filter.sharedMesh;
        } else {
            mesh = filter.mesh;
        }

        MeshGenerationResult result = new MeshGenerationResult();
        result.quads = new Dictionary<Vector3, Dictionary<Vector3, TerrainQuad>>();
        result.vertices = new List<Vector3>();
        result.uvs = new List<Vector2>();
        result.tris = new List<int>();
        for (int z = 0; z < size.y; z += 1) {
            for (int x = 0; x < size.x; x += 1) {
                // top vertices
                float height = HeightAt(x, z);
                AddQuad(result, new Vector3(x, height, z), new Vector3(x + 1, height, z + 1),
                    TileAt(x, z), new Vector3(x, height, z), new Vector3(0, 1, 0));

                // side vertices
                foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
                    float currentHeight = HeightAt(x, z);
                    float neighborHeight = HeightAt(x + dir.Px3DX(), z + dir.Px3DZ());
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
                            AddQuad(result, new Vector3(x + off1.x, y, z + off1.y),
                                new Vector3(x + off2.x, y + 0.5f, z + off2.y),
                                TileAt(x, z, y, dir),
                                new Vector3(x, y + 0.5f, z), dir.Px3D());
                        }
                    }
                }
            }
        }

        if (regenMesh) {
            mesh.Clear();

            mesh.vertices = result.vertices.ToArray();
            mesh.triangles = result.tris.ToArray();
            mesh.uv = result.uvs.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
        return result;
    }

    private void AddQuad(MeshGenerationResult result, Vector3 lowerLeft, Vector3 upperRight, 
            Tile tile, Vector3 pos, Vector3 normal) {
        TerrainQuad quad = new TerrainQuad(result.tris, result.vertices, result.uvs, 
            lowerLeft, upperRight, tile, tileset, pos, normal);
        if (!result.quads.ContainsKey(pos)) {
            result.quads[pos] = new Dictionary<Vector3, TerrainQuad>();
        }
        result.quads[pos][normal] = quad;
    }

    // === EDITOR-Y THINGS =========================================================================

    public float HeightAt(Vector2Int pos) {
        return HeightAt(pos.x, pos.y);
    }
    public float HeightAt(int x, int y) {
        if (x < 0 || x >= size.x || y < 0 || y >= size.y) {
            return 0;
        } else {
            return heights[y * size.x + x];
            
        }
    }

    public void SetHeight(int x, int y, float height) {
        heights[y * size.x + x] = height;
    }

    public Tile TileAt(int x, int y) {
        Tile tile = tilemap.GetTile<Tile>(new Vector3Int(x, y, 0));
        if (tile == null) {
            return defaultTopTile;
        } else {
            return tile;
        }
    }

    public Tile TileAt(int x, int y, float height, OrthoDir dir) {
        FacingTileKey key = new FacingTileKey();
        key.dir = dir;
        key.pos = new Vector3(x, height, y);
        if (facingTiles.ContainsKey(key)) {
            return facingTiles[key];
        } else {
            return defaultFaceTile;
        }
    }

    public void SetTile(int x, int y, Tile tile) {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void SetTile(int x, int y, float height, OrthoDir dir, Tile tile) {
        FacingTileKey key = new FacingTileKey();
        key.dir = dir;
        key.pos = new Vector3(x, height, y);
        facingTiles[key] = tile;
    }

    // === SERIALIZATION ===========================================================================

    public void OnBeforeSerialize() {
        serializedFacingTiles = new FacingTileDictionary(facingTiles);
    }

    public void OnAfterDeserialize() {
        if (serializedFacingTiles != null) {
            facingTiles = serializedFacingTiles.ToDictionary();
        } else {
            facingTiles = new Dictionary<FacingTileKey, Tile>();
        }
    }

    [System.Serializable]
    public struct FacingTileKey {
        public Vector3 pos;
        public OrthoDir dir;
    }

    [System.Serializable]
    public class FacingTileDictionary : SerialDictionary<FacingTileKey, Tile> {
        public FacingTileDictionary(Dictionary<FacingTileKey, Tile> dictionary) : base(dictionary) {

        }
    }
}
