using UnityEngine;
using UnityEngine.Tilemaps;

public class Map2DTile : Tile, PropertiedTile {

    public TilePropertyData properties;

    public Map2DTile(TilePropertyData properties) {
        this.properties = properties;
    }

    public TilePropertyData GetData() {
        return properties;
    }

    public bool EqualsTile(TileBase tile) {
        if (tile == null || !(tile is Map2DTile)) {
            return false;
        }
        Map2DTile other = (Map2DTile)tile;
        return sprite == other.sprite;
    }

    public Sprite GetSprite() {
        return sprite;
    }
}
