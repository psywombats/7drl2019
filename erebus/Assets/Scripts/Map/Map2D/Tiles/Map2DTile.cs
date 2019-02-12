using UnityEngine;
using UnityEngine.Tilemaps;

public class Map2DTile : Tile, PropertiedTile {

    public TilePropertyData properties;

    public TilePropertyData GetData() {
        return properties;
    }
}
