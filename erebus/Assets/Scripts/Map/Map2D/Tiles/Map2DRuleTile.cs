using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Rule Tile", menuName = "Map2DTiles/Rule Tile")]
public class Map2DRuleTile : RuleTile, PropertiedTile {

    public TilePropertyData properties;

    public TilePropertyData GetData() {
        return properties;
    }
}
