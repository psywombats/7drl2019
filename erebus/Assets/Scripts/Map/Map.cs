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

    public static readonly IntVector2 TileSizePx = new IntVector2(16, 16);
    public static int TileWidthPx { get { return (int)TileSizePx.x; } }
    public static int TileHeightPx { get { return (int)TileSizePx.y; } }

    public IntVector2 Size;
    public int Width { get { return Size.x; } }
    public int Height { get { return Size.y; } }

    public IntVector2 SizePx;
    public int WidthPx { get { return SizePx.x; } }
    public int HeightPx { get { return SizePx.y; } }

    public string BGMKey { get; private set; }
    public String ResourcePath { get { return GetComponent<TiledMap>().ResourcePath; } }
    public string FullName {
        get {
            string name = gameObject.name;
            if (name.EndsWith("(Clone)")) {
                name = name.Substring(0, name.Length - "(Clone)".Length);
            }
            return ResourcePath + "/" + name;
        }
    }

    public override void Populate(IDictionary<string, string> properties) {
        TiledMap tiled = GetComponent<TiledMap>();
        Size = new IntVector2(tiled.NumTilesWide, tiled.NumTilesHigh);
        SizePx = IntVector2.Scale(Size, TileSizePx);

        if (properties.ContainsKey(PropertyBGM)) {
            BGMKey = properties[PropertyBGM];
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
        if (BGMKey != null) {
            Global.Instance().Audio.PlayBGM(BGMKey);
        }
    }

    public void OnTeleportAway() {

    }

    // returns a list of coordinates to step to with the last one being the destination, or null if no path
    public List<IntVector2> FindPath(CharaEvent actor, IntVector2 to) {
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
            visited.Add(at);

            if (at == to) {
                // trim to remove the current location from the beginning
                return head.GetRange(1, head.Count - 1);
            }

            foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
                IntVector2 next = head[head.Count - 1] + dir.XY();
                if (!visited.Contains(next) && actor.CanPassAt(next)) {
                    List<IntVector2> newHead = new List<IntVector2>(head);
                    newHead.Add(next);
                    heads.Add(newHead);
                }
            }
        }

        return null;
    }
}
