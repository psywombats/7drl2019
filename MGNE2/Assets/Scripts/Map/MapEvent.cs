using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * The generic "thing on the map" class for MGNE2. Usually comes from Tiled.
 */
public class MapEvent : TiledInstantiated {

    public IntVector2 Position;
    public Vector2 PositionPx {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }
    public Vector2 TargetPosition {
        get;
        private set;
    }

    public Map Parent {
        get {
            GameObject parent = gameObject;
            do {
                parent = parent.transform.parent.gameObject;
                Map map = parent.GetComponent<Map>();
                if (map != null) {
                    return map;
                }
            } while (parent.transform.parent != null);
            return null;
        }
    }

    public float PixelsPerSecond = 32.0f;

    private bool tracking;

    public override void Populate(IDictionary<string, string> properties) {
        Position = new IntVector2(0, 0);
        RectangleObject rect = GetComponent<RectangleObject>();
        if (rect != null) {
            Position.Set((int)rect.TmxPosition.x / Map.TileWidthPx, (int)(Parent.HeightPx - rect.TmxPosition.y - Map.TileHeightPx) / Map.TileHeightPx);
        }
    }

    public void Update() {
        if (tracking) {
            PositionPx = Vector2.MoveTowards(PositionPx, TargetPosition, PixelsPerSecond * Time.deltaTime);
            Vector2 position2 = PositionPx;
            if (position2 == TargetPosition) {
                tracking = false;
            }
        }
    }

    public void Step(OrthoDir dir) {
        if (!tracking) {
            tracking = true;
            Position += dir.XY();
            TargetPosition = PositionPx + Vector2.Scale(dir.PxXY(), Map.TileSizePx);
        }
    }
}
