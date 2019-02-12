using UnityEngine.Tilemaps;

public interface PropertiedTile {

    TilePropertyData GetData();

    bool EqualsTile(TileBase tile);
}
