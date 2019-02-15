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
    
    public string fullName;
    public Grid grid;

    public string bgmKey { get; private set; }
    
    // true if the tile in question is passable at x,y
    private Dictionary<Tilemap, bool[,]> passabilityMap;

    private IntVector2 _size;
    public IntVector2 size {
        get {
            if (_size.x == 0) {
                Vector3Int v3 = grid.transform.GetChild(0).GetComponent<Tilemap>().size;
                _size = new IntVector2(v3.x, v3.y);
            }
            return _size;
        }
    }
    public int width { get { return size.x; } }
    public int height { get { return size.y; } }

    private List<Tilemap> _layers;
    public List<Tilemap> layers {
        get {
            if (_layers == null) {
                _layers = new List<Tilemap>();
                foreach (Transform child in grid.transform) {
                    if (child.GetComponent<Tilemap>()) {
                        _layers.Add(child.GetComponent<Tilemap>());
                    }
                }
            }
            return _layers;
        }
    }

    public void Start() {
        // TODO: figure out loading
        Global.Instance().Maps.ActiveMap = this;
    }

    public Vector3Int TileToTilemapCoords(IntVector2 loc) {
        return TileToTilemapCoords(loc.x, loc.y);
    }

    public Vector3Int TileToTilemapCoords(int x, int y) {
        return new Vector3Int(x, -1 * (y + 1), 0);
    }

    public PropertiedTile TileAt(Tilemap layer, int x, int y) {
        return (PropertiedTile)layer.GetTile(TileToTilemapCoords(x, y));
    }

    public bool IsChipPassableAt(Tilemap layer, IntVector2 loc) {
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
    public List<MapEvent> GetEventsAt(ObjectLayer layer, IntVector2 loc) {
        List<MapEvent> events = new List<MapEvent>();
        foreach (MapEvent mapEvent in layer.gameObject.GetComponentsInChildren<MapEvent>()) {
            if (mapEvent.ContainsPosition(loc)) {
                events.Add(mapEvent);
            }
        }
        return events;
    }

    // returns the first event at loc that implements T
    public T GetEventAt<T>(ObjectLayer layer, IntVector2 loc) {
        List<MapEvent> events = GetEventsAt(layer, loc);
        foreach (MapEvent mapEvent in events) {
            if (mapEvent.GetComponent<T>() != null) {
                return mapEvent.GetComponent<T>();
            }
        }
        return default(T);
    }

    // returns all events that have a component of type t
    public List<T> GetEvents<T>() {
        return new List<T>(LowestObjectLayer().GetComponentsInChildren<T>());
    }

    public ObjectLayer LowestObjectLayer() {
        return GetComponentsInChildren<ObjectLayer>()[0];
    }

    public Tilemap TileLayerAtIndex(int layerIndex) {
        return GetComponentsInChildren<Tilemap>()[layerIndex];
    }

    public MapEvent GetEventNamed(string eventName) {
        foreach (ObjectLayer layer in GetComponentsInChildren<ObjectLayer>()) {
            foreach (MapEvent mapEvent in layer.GetComponentsInChildren<MapEvent>()) {
                if (mapEvent.name == eventName) {
                    return mapEvent;
                }
            }
        }
        return null;
    }

    public void OnTeleportTo() {
        if (bgmKey != null) {
            Global.Instance().Audio.PlayBGM(bgmKey);
        }
    }

    public void OnTeleportAway() {

    }

    // returns a list of coordinates to step to with the last one being the destination, or null
    public List<IntVector2> FindPath(CharaEvent actor, IntVector2 to) {
        return FindPath(actor, to, width > height ? width : height);
    }
    public List<IntVector2> FindPath(CharaEvent actor, IntVector2 to, int maxPathLength) {
        if (IntVector2.ManhattanDistance(actor.GetComponent<MapEvent>().position, to) > maxPathLength) {
            return null;
        }
        if (!actor.CanPassAt(to)) {
            return null;
        }

        HashSet<IntVector2> visited = new HashSet<IntVector2>();
        List<List<IntVector2>> heads = new List<List<IntVector2>>();
        List<IntVector2> firstHead = new List<IntVector2>();
        firstHead.Add(actor.GetComponent<MapEvent>().position);
        heads.Add(firstHead);

        while (heads.Count > 0) {
            heads.Sort(delegate (List<IntVector2> pathA, List<IntVector2> pathB) {
                int pathACost = pathA.Count + IntVector2.ManhattanDistance(pathA[pathA.Count - 1], to);
                int pathBCost = pathB.Count + IntVector2.ManhattanDistance(pathB[pathB.Count - 1], to);
                return pathACost.CompareTo(pathBCost);
            });
            List<IntVector2> head = heads[0];
            heads.RemoveAt(0);
            IntVector2 at = head[head.Count - 1];

            if (at == to) {
                // trim to remove the current location from the beginning
                return head.GetRange(1, head.Count - 1);
            }

            if (head.Count < maxPathLength) {
                foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
                    IntVector2 next = head[head.Count - 1];
                    // minor perf here, this is critical code
                    switch (dir) {
                        case OrthoDir.East:     next.x += 1;    break;
                        case OrthoDir.North:    next.y += 1;    break;
                        case OrthoDir.West:     next.x -= 1;    break;
                        case OrthoDir.South:    next.y -= 1;    break;
                    }
                    if (!visited.Contains(next) && actor.CanPassAt(next)) {
                        List<IntVector2> newHead = new List<IntVector2>(head);
                        newHead.Add(next);
                        heads.Add(newHead);
                        visited.Add(next);
                    }
                }
            }
        }

        return null;
    }
}
