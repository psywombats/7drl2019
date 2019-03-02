using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class MapGenerator {

    public Vector2Int sizeInRooms { get; private set; }
    public Vector2Int sizeInCells { get; private set; }
    public CellInfo[,] cells { get; private set; }
    public RoomInfo[,] rooms { get; private set; }
    public int[] widths { get; private set; }
    public int[] heights { get; private set; }

    public void GenerateMesh(TacticsTerrainMesh mesh) {
        int seed = (int)DateTime.Now.Ticks;
        Random.InitState(seed);
        Debug.Log("using seed " + seed);

        sizeInRooms = new Vector2Int(2, 2);
        rooms = new RoomInfo[sizeInRooms.x, sizeInRooms.y];

        sizeInCells = sizeInRooms * 2 - new Vector2Int(1, 1);
        cells = new CellInfo[sizeInCells.x, sizeInCells.y];

        // determine the size and start of each room and hallway
        cells = new CellInfo[sizeInCells.x, sizeInCells.y];
        widths = new int[sizeInCells.x];
        heights = new int[sizeInCells.y];
        for (int y = 0; y < sizeInCells.y; y += 1) {
            if (y % 2 == 1) {
                heights[y] = RandomHallSize();
            } else {
                heights[y] = RandomRoomSize();
            }
        }
        for (int x = 0; x < sizeInCells.x ; x += 1) {
            if (x % 2 == 1) {
                widths[x] = RandomHallSize();
            } else {
                widths[x] = RandomRoomSize();
            }
        }
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                cells[x, y].sizeX = widths[x];
                cells[x, y].sizeY = heights[y];
                cells[x, y].x = x;
                cells[x, y].y = y;
                if (x % 2 == 1 || y % 2 == 1) {
                    if (x % 2 == 1 && y % 2 == 1) {
                        cells[x, y].type = CellInfo.CellType.Pillar;
                    } else {
                        cells[x, y].type = CellInfo.CellType.Hall;
                    }
                } else {
                    cells[x, y].type = CellInfo.CellType.Room;
                }
                if (x == 0) {
                    cells[x, y].startX = 0;
                } else {
                    cells[x, y].startX = cells[x - 1, y].startX + widths[x - 1];
                }
                if (y == 0) {
                    cells[x, y].startY = 0;
                } else {
                    cells[x, y].startY = cells[x, y - 1].startY + heights[y - 1];
                }
            }
        }

        // determine the elevation of each room
        for (int diag = 0; diag < sizeInRooms.x + sizeInRooms.y; diag += 1) {
            int x = diag;
            int y = 0;
            while (x >= 0) {
                if (x < sizeInRooms.x && y < sizeInRooms.y) {
                    int z;
                    if (x == 0 && y == 0) {
                        z = 1;
                    } else {
                        int oldZ = (x == 0 || (Flip() && y != 0)) ? rooms[x, y - 1].z : rooms[x - 1, y].z;
                        z = RandomNewZ(oldZ);
                    }
                    rooms[x, y].z = z;
                }
                x -= 1;
                y += 1;
            }
        }

        // render terrain
        CellInfo cornerCell = cells[sizeInCells.x - 1, sizeInCells.y - 1];
        mesh.Resize(new Vector2Int(cornerCell.startX + cornerCell.sizeX, cornerCell.startY + cornerCell.sizeY));
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                CellInfo cell = cells[x, y];
                if (cell.type != CellInfo.CellType.Pillar) {
                    cell.RenderTerrain(this, mesh);
                }
            }
        }
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                CellInfo cell = cells[x, y];
                if (cell.type == CellInfo.CellType.Pillar) {
                    cell.RenderTerrain(this, mesh);
                }
            }
        }
    }

    private bool Flip() {
        return Random.Range(0.0f, 1.0f) > 0.5f;
    }

    private int RandomHallSize() {
        int h;
        float r = Random.Range(0.0f, 1.0f);
        if      (r < 0.5)   h = 2;
        else if (r < 0.75)  h = 3;
        else if (r < 0.95)  h = 1;
        else                h = 5;
        return h;
    }

    private int RandomRoomSize() {
        // we're standardizing rooms at 7*7 so as to be able to fill them with hand stuff
        return 7;
    }

    private int RandomNewZ(int oldZ) {
        int z;
        float r = Random.Range(0.0f, 1.0f);
        if      (r < 0.15)  z = oldZ - 1;
        else if (r < 0.3)   z = oldZ;
        else if (r < 0.6)   z = oldZ + 2;
        else if (r < 0.9)   z = oldZ + 3;
        else                z = oldZ + 4;
        return Math.Max(z, 1);
    }
}
