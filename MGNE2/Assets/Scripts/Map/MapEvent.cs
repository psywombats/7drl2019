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

    public override void Populate(IDictionary<string, string> properties) {
        Position = new IntVector2(0, 0);
        RectangleObject rect = GetComponent<RectangleObject>();
        if (rect != null) {
            Position.Set((int)rect.TmxPosition.x / Map.TileWidthPx, (int)(Parent.HeightPx - rect.TmxPosition.y - Map.TileHeightPx) / Map.TileHeightPx);
        }
    }
}
