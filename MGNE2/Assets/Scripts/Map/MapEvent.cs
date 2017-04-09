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

    public IntVector2 Position;
    public Vector2 PositionPx {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }
    public Vector2 TargetPosition { get; set; }
    public Dispatch EventDispatch { get { return GetComponent<Dispatch>(); } }

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
        transform.x *= (OrthoDir.East.X() != OrthoDir.East.PxX()) ? -1 : 1;
        transform.y *= (OrthoDir.North.Y() != OrthoDir.North.PxY()) ? -1 : 1;
        PositionPx = Vector2.Scale(Position, transform);
        SetDepth();
    }

    public void Update() {
        SetDepth();
    }

    public void SetDepth() {
        if (Parent != null) {
            for (int i = 0; i < Parent.transform.childCount; i += 1) {
                if (Layer == Parent.transform.GetChild(i).gameObject.GetComponent<ObjectLayer>()) {
                    Rect mapRect = Parent.GetComponent<TiledMap>().GetMapRect();
                    float depthPerLayer = -1 * Map.TileHeightPx / mapRect.height;
                    float z = (PositionPx.y / mapRect.height) + (depthPerLayer * i);
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z);
                }
            }
        }
    }
}
