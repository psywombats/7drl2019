using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class RoomInfo {

    private const string VaultsPath = "Maps/Vaults/Index";

    private static VaultList vaults;

    public CellInfo cell;
    public float z;

    public RoomInfo(CellInfo cell, float z) {
        this.cell = cell;
        this.z = z;
    }

    public void FillWithVault(TacticsTerrainMesh mesh) {
        if (vaults == null) {
            vaults = Resources.Load<VaultList>(VaultsPath);
        }
        bool flipX = RandUtils.Flip();
        bool flipY = RandUtils.Flip();
        TacticsTerrainMesh vault = vaults.vaults[Random.Range(0, vaults.vaults.Count)];
        for (int trueY = 0; trueY < cell.sizeY; trueY += 1) {
            for (int trueX = 0; trueX < cell.sizeX; trueX += 1) {
                int x = flipX ? trueX : (cell.sizeX - 1 - trueX);
                int y = flipY ? trueY : (cell.sizeY - 1 - trueY);
                mesh.SetHeight(cell.startX + x, cell.startY + y, 
                    mesh.HeightAt(cell.startX + x, cell.startY + y) + vault.HeightAt(x, y) - 1.0f);
                Tile topTile = vault.TileAt(x, y);
                if (topTile != mesh.defaultTopTile) {
                    mesh.SetTile(cell.startX + x, cell.startY + y, topTile);
                }
                for (float h = 0.0f; h < vault.HeightAt(x, y); h += 0.5f) {
                    foreach (OrthoDir dir in System.Enum.GetValues(typeof(OrthoDir))) {
                        Tile t = vault.TileAt(x, y, h, dir);
                        if (t != mesh.defaultFaceTile) {
                            mesh.SetTile(cell.startX + x, cell.startY + y, mesh.HeightAt(x, y) - 1 + h, dir, t);
                        }
                    }
                }
            }
        }
    }

    public override string ToString() {
        return "[" + cell.x / 2 + "," + cell.y / 2 + "]";
    }
}
