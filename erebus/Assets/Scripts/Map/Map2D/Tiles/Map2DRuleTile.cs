using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
[CreateAssetMenu(fileName = "New Rule Tile", menuName = "Map2DTiles/Rule Tile")]
public class Map2DRuleTile : RuleTile, PropertiedTile {

    public TilePropertyData properties;

    public bool EqualsTile(TileBase tile) {
        if (tile == null || !(tile is Map2DRuleTile)) {
            return false;
        }
        Map2DRuleTile other = (Map2DRuleTile)tile;
        return this.m_DefaultSprite == other.m_DefaultSprite;
    }

    public TilePropertyData GetData() {
        return properties;
    }
}
