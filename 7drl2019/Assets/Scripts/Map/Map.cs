using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * MGNE's big map class, now in MGNE2. Converted from Tiled and now to unity maps.
 */
public class Map : MonoBehaviour {

    // some game-wide critical map2d constants
    public const int TileSizePx = 16;
    public const int UnityUnitScale = 1;

    public const string ResourcePath = "Maps/";
    
    public Grid grid;
    public ObjectLayer objectLayer;

    public string bgmKey { get; private set; }
    
    // true if the tile in question is passable at x,y
    private Dictionary<Tilemap, bool[,]> passabilityMap;

    private Vector2Int _size;
    public Vector2Int size {
        get {
            if (_size.x == 0) {
                if (terrain != null) {
                    _size = GetComponent<TacticsTerrainMesh>().size;
                } else {
                    Vector3Int v3 = grid.transform.GetChild(0).GetComponent<Tilemap>().size;
                    _size = new Vector2Int(v3.x, v3.y);
                }
            }
            return _size;
        }
    }
    public Vector2 sizePx { get { return size * TileSizePx; } }
    public int width { get { return size.x; } }
    public int height { get { return size.y; } }

    private List<Tilemap> _layers;
    public List<Tilemap> layers {
        get {
            if (_layers == null) {
                _layers = new List<Tilemap>();
                if (terrain != null) {
                    _layers.Add(GetComponent<Tilemap>());
                } else {
                    foreach (Transform child in grid.transform) {
                        if (child.GetComponent<Tilemap>()) {
                            _layers.Add(child.GetComponent<Tilemap>());
                        }
                    }
                }
            }
            return _layers;
        }
    }

    private TacticsTerrainMesh _terrain;
    public TacticsTerrainMesh terrain {
        get {
            if (_terrain == null) _terrain = GetComponent<TacticsTerrainMesh>();
            return _terrain;
        }
    }

    public void Start() {
        // TODO: figure out loading
        Global.Instance().Maps.activeMap = this;
    }

    public Vector3Int TileToTilemapCoords(Vector2Int loc) {
        return TileToTilemapCoords(loc.x, loc.y);
    }

    public Vector3Int TileToTilemapCoords(int x, int y) {
        return new Vector3Int(x, -1 * (y + 1), 0);
    }

    public PropertiedTile TileAt(Tilemap layer, int x, int y) {
        return (PropertiedTile)layer.GetTile(TileToTilemapCoords(x, y));
    }

    public bool IsChipPassableAt(Tilemap layer, Vector2Int loc) {
        if (passabilityMap == null) {
            passabilityMap = new Dictionary<Tilemap, bool[,]>();
        }
        if (!passabilityMap.ContainsKey(layer)) {
            passabilityMap[layer] = new bool[width, height];
            for (int x = 0; x < width; x += 1) {
                for (int y = 0; y < height; y += 1) {
                    PropertiedTile tile = TileAt(layer, x, y);
                    passabilityMap[layer][x, y] = tile == null || tile.GetData().passable;
                }
            }
        }

        return passabilityMap[layer][loc.x, loc.y];
    }

    // careful, this implementation is straight from MGNE, it's efficiency is questionable, to say the least
    // it does support bigger than 1*1 events though
    public List<MapEvent> GetEventsAt(Vector2Int loc) {
        List<MapEvent> events = new List<MapEvent>();
        foreach (MapEvent mapEvent in objectLayer.GetComponentsInChildren<MapEvent>()) {
            if (mapEvent.ContainsPosition(loc)) {
                events.Add(mapEvent);
            }
        }
        return events;
    }

    // returns the first event at loc that implements T
    public T GetEventAt<T>(Vector2Int loc) {
        List<MapEvent> events = GetEventsAt(loc);
        foreach (MapEvent mapEvent in events) {
            if (mapEvent.GetComponent<T>() != null) {
                return mapEvent.GetComponent<T>();
            }
        }
        return default(T);
    }

    // returns all events that have a component of type t
    public List<T> GetEvents<T>() {
        return new List<T>(objectLayer.GetComponentsInChildren<T>());
    }

    public Tilemap TileLayerAtIndex(int layerIndex) {
        return GetComponentsInChildren<Tilemap>()[layerIndex];
    }

    public MapEvent GetEventNamed(string eventName) {
        foreach (ObjectLayer layer in GetComponentsInChildren<ObjectLayer>()) {
            foreach (MapEvent mapEvent in layer.GetComponentsInChildren<MapEvent>()) {
                if (mapEvent.name.StartsWith(eventName)) {
                    return mapEvent;
                }
            }
        }
        return null;
    }

    public void AddEvent(MapEvent toAdd) {
        toAdd.transform.SetParent(objectLayer.transform);
        toAdd.SetScreenPositionToMatchTilePosition();
    }

    public void RemoveEvent(MapEvent toRemove, bool allowEditDelete = false) {
        // i dunno
        if (!Application.isPlaying && allowEditDelete) {
            DestroyImmediate(toRemove.gameObject);
        } else {
            Destroy(toRemove.gameObject);
        }
        toRemove.gameObject.SetActive(false);
        toRemove.gameObject.name = "<deleted>";
    }

    public void OnTeleportTo() {
        if (bgmKey != null) {
            Global.Instance().Audio.PlayBGM(bgmKey);
        }
    }

    public void OnTeleportAway() {

    }

    // returns a list of coordinates to step to with the last one being the destination, or null
    public List<Vector2Int> FindPath(MapEvent actor, Vector2Int to) {
        return FindPath(actor, to, width > height ? width : height);
    }
    public List<Vector2Int> FindPath(MapEvent actor, Vector2Int to, int maxPathLength) {
        if (Vector2.Distance(actor.GetComponent<MapEvent>().location, to) * Mathf.Sqrt(2) > maxPathLength) {
            return null;
        }
        if (!IsChipPassableAt(layers[0], to)) {
            return null;
        }

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<List<Vector2Int>> heads = new List<List<Vector2Int>>();
        List<Vector2Int> firstHead = new List<Vector2Int>();
        firstHead.Add(actor.GetComponent<MapEvent>().location);
        heads.Add(firstHead);

        while (heads.Count > 0) {
            heads.Sort(delegate (List<Vector2Int> pathA, List<Vector2Int> pathB) {
                int pathACost = pathA.Count + ManhattanDistance(pathA[pathA.Count - 1], to);
                int pathBCost = pathB.Count + ManhattanDistance(pathB[pathB.Count - 1], to);
                return pathACost.CompareTo(pathBCost);
            });
            List<Vector2Int> head = heads[0];
            heads.RemoveAt(0);
            Vector2Int at = head[head.Count - 1];

            if (at == to) {
                // trim to remove the current location from the beginning
                return head.GetRange(1, head.Count - 1);
            }

            if (head.Count < maxPathLength) {
                foreach (EightDir dir in Enum.GetValues(typeof(EightDir))) {
                    Vector2Int next = head[head.Count - 1];
                    // minor perf here, this is critical code
                    switch (dir) {
                        case EightDir.N:    next.y += 1;                    break;
                        case EightDir.E:    next.x += 1;                    break;
                        case EightDir.S:    next.y -= 1;                    break;
                        case EightDir.W:    next.x -= 1;                    break;
                        case EightDir.NE:   next.y += 1;    next.x += 1;    break;
                        case EightDir.SE:   next.y -= 1;    next.x += 1;    break;
                        case EightDir.SW:   next.y -= 1;    next.x -= 1;    break;
                        case EightDir.NW:   next.y += 1;    next.x -= 1;    break;
                    }
                    if (next == to || (!visited.Contains(next) && actor.CanPassAt(next) &&
                        (actor.GetComponent<CharaEvent>() == null ||
                             actor.CanPassAt(next)) &&
                        (actor.GetComponent<BattleEvent>() == null ||
                             actor.GetComponent<BattleEvent>().CanCrossTileGradient(at, next)))) {

                        List<Vector2Int> newHead = new List<Vector2Int>(head) { next };

                        if (next != to || GetEventAt<BattleEvent>(to) == null ||
                            Mathf.Abs(terrain.HeightAt(at) - terrain.HeightAt(to)) <= BattleEvent.AttackHeightMax) { 

                            heads.Add(newHead);
                            visited.Add(next);
                        }
                    }
                }
            }
        }

        return null;
    }

    private static int ManhattanDistance(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
