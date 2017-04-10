using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

/**
 * MGNE's big map class, now in MGNE2. Converted from Tiled.
 */
public class Map : TiledInstantiated {

    public static readonly IntVector2 TileSizePx = new IntVector2(16, 16);
    public static int TileWidthPx { get { return (int)TileSizePx.x; } }
    public static int TileHeightPx { get { return (int)TileSizePx.y; } }

    public IntVector2 Size;
    public int Width { get { return Size.x; } }
    public int Height { get { return Size.y; } }

    public IntVector2 SizePx;
    public int WidthPx { get { return SizePx.x; } }
    public int HeightPx { get { return SizePx.y; } }

    public override void Populate(IDictionary<string, string> properties) {
        TiledMap tiled = GetComponent<TiledMap>();
        Size = new IntVector2(tiled.NumTilesWide, tiled.NumTilesHigh);
        SizePx = IntVector2.Scale(Size, TileSizePx);
    }

    public bool PassableAt(TileLayer layer, IntVector2 loc) {
        TiledMap tiledMap = GetComponent<TiledMap>();
        TiledProperty property = tiledMap.GetPropertyForTile("x", layer, loc.x, loc.y);
        return (property == null) ? true : (property.GetStringValue() == "false");
    }
}
