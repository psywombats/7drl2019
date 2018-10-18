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

    public const int TileSizePx = 16;

    public IntVector2 size;
    public int width { get { return size.x; } }
    public int height { get { return size.y; } }

    public string bgmKey { get; private set; }
    public String resourcePath { get { return GetComponent<TiledMap>().ResourcePath; } }
    public string fullName {
        get {
            string name = gameObject.name;
            if (name.EndsWith("(Clone)")) {
                name = name.Substring(0, name.Length - "(Clone)".Length);
            }
            return resourcePath + "/" + name;
        }
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
    }

    public bool IsChipPassableAt(TileLayer layer, IntVector2 loc) {
        TiledMap tiledMap = GetComponent<TiledMap>();
        TiledProperty property = tiledMap.GetPropertyForTile("x", layer, loc.x, loc.y);
        return (property == null) ? true : (property.GetStringValue() == "false");
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
        if (IntVector2.ManhattanDistance(actor.GetComponent<MapEvent>().Position, to) > maxPathLength) {
            return null;
        }
        if (!actor.CanPassAt(to)) {
            return null;
        }

        HashSet<IntVector2> visited = new HashSet<IntVector2>();
        List<List<IntVector2>> heads = new List<List<IntVector2>>();
        List<IntVector2> firstHead = new List<IntVector2>();
        firstHead.Add(actor.GetComponent<MapEvent>().Position);
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
                    IntVector2 next = head[head.Count - 1] + dir.XY();
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
