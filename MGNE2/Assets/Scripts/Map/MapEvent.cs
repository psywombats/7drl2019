using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * The generic "thing on the map" class for MGNE2. Usually comes from Tiled.
 */
 [RequireComponent(typeof(Dispatch))]
public class MapEvent : TiledInstantiated {

    // Editor properties

    public IntVector2 Position;

    [TextArea(3, 6)]
    public string LuaOnInteract;

    [TextArea(3, 6)]
    public string LuaOnCollide;

    [TextArea(2, 2)]
    public string LuaCondition;

    // Properties

    public Vector2 PositionPx {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }
    public Vector2 TargetPosition { get; set; }

    public Map Parent {
        get {
            GameObject parent = gameObject;
            while (parent.transform.parent != null) {
                parent = parent.transform.parent.gameObject;
                Map map = parent.GetComponent<Map>();
                if (map != null) {
                    return map;
                }
            }
            return null;
        }
    }

    public ObjectLayer Layer {
        get {
            GameObject parent = gameObject;
            do {
                parent = parent.transform.parent.gameObject;
                ObjectLayer layer = parent.GetComponent<ObjectLayer>();
                if (layer != null) {
                    return layer;
                }
            } while (parent.transform.parent != null);
            return null;
        }
    }

    public override void Populate(IDictionary<string, string> properties) {
        gameObject.AddComponent<Dispatch>();
        Position = new IntVector2(0, 0);
        RectangleObject rect = GetComponent<RectangleObject>();
        if (rect != null) {
            Position.Set((int)rect.TmxPosition.x / Map.TileWidthPx, (int)rect.TmxPosition.y / Map.TileHeightPx);
        }

        SetDepth();
    }

    public void OnValidate() {
        Vector2 transform = Map.TileSizePx;
        if (OrthoDir.East.X() != OrthoDir.East.PxX()) {
            transform.x = transform.x * -1;
        }
        if (OrthoDir.North.Y() != OrthoDir.North.PxY()) {
            transform.y = transform.y * -1;
        }
        PositionPx = Vector2.Scale(Position, transform);
        if (OrthoDir.East.X() != OrthoDir.East.PxX()) {
            PositionPx = new Vector2(PositionPx.x - Map.TileWidthPx, PositionPx.y);
        }
        if (OrthoDir.North.Y() != OrthoDir.North.PxY()) {
            PositionPx = new Vector2(PositionPx.x, PositionPx.y - Map.TileHeightPx);
        }
        SetDepth();
    }

    public void Update() {
        SetDepth();
    }

    public void SetDepth() {
        if (Parent != null) {
            for (int i = 0; i < Parent.transform.childCount; i += 1) {
                if (Layer == Parent.transform.GetChild(i).gameObject.GetComponent<ObjectLayer>()) {
                    float depthPerLayer = -1.0f;
                    float z = depthPerLayer * ((float)Position.y / (float)Parent.Height) + (depthPerLayer * (float)i);
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z);
                }
            }
        }
    }

    public bool IsPassableBy(CharaEvent chara) {
        // right now all non-chara events are passable
        return GetComponent<CharaEvent>() == null;
    }
}
