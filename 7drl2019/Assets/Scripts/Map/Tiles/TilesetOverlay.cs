using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesetOverlay : MonoBehaviour {

    [Space]
    [Header("Controls")]
    public GameObject tilesetPrefab;
    public PaintMode mode = PaintMode.Passability;

    [Space]
    [Header("Tileset")]
    public TileBase tileX;
    public TileBase tileO;

    [Space]
    [Header("Boring junk")]
    public GameObject anchorPoint;
    public Tilemap overlay;

    public enum PaintMode {
        Passability,
    }

    public void OnValidate() {
        if (tilesetPrefab != null) {
            GameObject child = anchorPoint.transform.GetChild(0).gameObject;
            if (child == null || !child.name.Equals(tilesetPrefab.name)) {
                if (child != null) {
                    DestroyImmediate(child);
                }
                GameObject tileset = Instantiate(tilesetPrefab);
                tileset.transform.parent = anchorPoint.transform;
                tileset.transform.localPosition = Vector3.zero;
            }
            UpdateMap();
        }
    }

    public void UpdateMap() {
        for (int y = 0; y < overlay.size.y; y += 1) {
            for (int x = 0; x < overlay.size.x; x += 1) {
                overlay.SetTile(new Vector3Int(x, y, 0), null);
            }
        }

        Tilemap tileset = tilesetPrefab.GetComponentInChildren<Tilemap>();
        for (int y = 0; y < tileset.size.y; y += 1) {
            for (int x = 0; x < tileset.size.x; x += 1) {
                Vector3Int loc = new Vector3Int(x, y, 0);
                PropertiedTile tile = (PropertiedTile)tileset.GetTile(loc);
                TileBase overlayTile = null;
                switch (mode) {
                    case PaintMode.Passability:
                        overlayTile = tile.GetData().impassable ? tileO : tileX;
                        break;
                }
                overlay.SetTile(loc, overlayTile);
            }
        }
    }

    public void CheckForChanges() {
        Tilemap tileset = tilesetPrefab.GetComponentInChildren<Tilemap>();
        for (int y = 0; y < overlay.size.y; y += 1) {
            for (int x = 0; x < overlay.size.x; x += 1) {
                Vector3Int loc = new Vector3Int(x, y, 0);
                PropertiedTile tile = (PropertiedTile)tileset.GetTile(loc);
                TileBase overlayTile = overlay.GetTile(loc);
                switch (mode) {
                    case PaintMode.Passability:
                        if (overlayTile == tileO && !tile.GetData().impassable) {
                            tile.GetData().impassable = true;
                        } else if (overlayTile == tileX && tile.GetData().impassable) {
                            tile.GetData().impassable = false;
                        }
                        break;
                }
            }
        }
        UpdateMap();
    }
}
