using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * MGNE's big map class, now in MGNE2. Converted from Tiled.
 */
[RequireComponent(typeof(TiledMap))]
public class Map : TiledInstantiated {

    private const string PropertyBGM = "bgm";
    private const string PropertyBattle = "battle";
    private const string PropertyLayer = "displayLayer";
    private const string PropertyType = "type";

    private const string TypeDuel = "duel";

    public const int TileSizePx = 16;

    public IntVector2 size;
    public string displayLayer;
    public int width { get { return size.x; } }
    public int height { get { return size.y; } }

    public string bgmKey { get; private set; }
    public string resourcePath { get { return GetComponent<TiledMap>().ResourcePath; } }
    public string fullName {
        get {
            string name = gameObject.name;
            if (name.EndsWith("(Clone)")) {
                name = name.Substring(0, name.Length - "(Clone)".Length);
            }
            return resourcePath + "/" + name;
        }
    }

    // true if the tile at x,y has the x "impassable" property for pathfinding
    private Dictionary<TileLayer, bool[,]> passabilityXMap;

    public void Start() {
        // TODO: figure out loading
        Global.Instance().Maps.ActiveMap = this;
    }

    public override void Populate(IDictionary<string, string> properties) {
        TiledMap tiled = GetComponent<TiledMap>();
        size = new IntVector2(tiled.NumTilesWide, tiled.NumTilesHigh);

        if (properties.ContainsKey(PropertyBGM)) {
            bgmKey = properties[PropertyBGM];
        }
        if (properties.ContainsKey(PropertyBattle)) {
            BattleController battleController = gameObject.AddComponent<BattleController>();
            battleController.Setup(properties[PropertyBattle]);
        }
        if (properties.ContainsKey(PropertyLayer)) {
            displayLayer = properties[PropertyLayer];
        }
        if (properties.ContainsKey(PropertyType)) {
            switch (properties[PropertyType]) {
                case TypeDuel:
                    gameObject.AddComponent<DuelMap>();
                    break;
                default:
                    Debug.Assert(false, "Unknown map type " + properties[PropertyType]);
                    break;
            }
        }
    }

    public bool IsChipPassableAt(TileLayer layer, IntVector2 loc) {
        TiledMap tiledMap = GetComponent<TiledMap>();
        if (passabilityXMap == null) {
            passabilityXMap = new Dictionary<TileLayer, bool[,]>();
        }
        if (!passabilityXMap.ContainsKey(layer)) {
            passabilityXMap[layer] = new bool[width, height];
            for (int x = 0; x < width; x += 1) {
                for (int y = 0; y < height; y += 1) {
                    TiledProperty property = tiledMap.GetPropertyForTile("x", layer, x, y);
                    passabilityXMap[layer][x, y] = (property == null) ? true : (property.GetStringValue() == "false");
                }
            }
        }

        return passabilityXMap[layer][loc.x, loc.y];
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

    public Layer LayerAtIndex(int layerIndex) {
        return transform.GetChild(layerIndex).GetComponent<Layer>();
    }

    public ObjectLayer LowestObjectLayer() {
        return GetComponentsInChildren<ObjectLayer>()[0];
    }

    public TileLayer TileLayerAtIndex(int layerIndex) {
        return GetComponentsInChildren<TileLayer>()[layerIndex];
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
