using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class MapGenerator {

    private int MaxHeightDelta = 3;

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

        // set the size and start of each room and hallway
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
                cells[x, y] = new CellInfo(x, y, widths[x], heights[y]);
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

        // set the elevation of each room
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
                    rooms[x, y] = new RoomInfo(cells[x * 2, y * 2], z);
                }
                x -= 1;
                y += 1;
            }
        }

        // determine the initial passability map
        bool dirty = true;
        cells[0, 0].connected = true;
        while (dirty) {
            dirty = false;
            for (int y = 0; y < sizeInRooms.y; y += 1) {
                for (int x = 0; x < sizeInRooms.x; x += 1) {
                    RoomInfo room = rooms[x, y];
                    if (room.cell.connected) {
                        continue;
                    }
                    foreach (RoomInfo other in room.cell.AdjacentRooms(this)) {
                        if (other.cell.connected && Math.Abs(room.z - other.z) <= MaxHeightDelta) {
                            room.cell.connected = true;
                            room.cell.passableDirs[(int)room.cell.DirTo(other.cell)] = true;
                            other.cell.passableDirs[(int)other.cell.DirTo(room.cell)] = true;
                            dirty = true;
                        }
                    }
                }
            }
        }

        // convert halls into stairways as needed
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                CellInfo cell = cells[x, y];
                if (cell.type == CellInfo.CellType.Hall) {
                    List<RoomInfo> rooms = cell.AdjacentRooms(this);
                    if (Math.Abs(rooms[0].z - rooms[1].z) > 1) {
                        cell.type = CellInfo.CellType.Stairway;
                    }
                }
            }
        }

        // set pillar heights to sane values
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                CellInfo cell = cells[x, y];
                if (cell.type == CellInfo.CellType.Pillar) {
                    CellInfo n = cells[x, y + 1];
                    CellInfo e = cells[x + 1, y];
                    CellInfo s = cells[x, y - 1];
                    CellInfo w = cells[x - 1, y];
                    if (n.type == CellInfo.CellType.Stairway && e.type == CellInfo.CellType.Stairway) {
                        cell.pillarZ = rooms[(x + 1) / 2, (y + 1) / 2].z;
                        e.stairAnchoredHigh = true;
                        e.passableDirs[(int)OrthoDir.West] = true;
                        cell.passableDirs[(int)OrthoDir.East] = true;
                        n.stairAnchoredHigh = true;
                        n.passableDirs[(int)OrthoDir.South] = true;
                        cell.passableDirs[(int)OrthoDir.North] = true;
                    } else if (Math.Abs(rooms[(x + 1) / 2, (y - 1) / 2].z - rooms[(x + 1) / 2, (y - 1) / 2].z) <= 1.0) {
                        cell.pillarZ = (rooms[(x + 1) / 2, (y - 1) / 2].z + rooms[(x - 1) / 2, (y + 1) / 2].z) / 2.0f;
                        if (n.type == CellInfo.CellType.Stairway) {
                            n.stairAnchoredLow = true;
                            n.passableDirs[(int)OrthoDir.South] = true;
                            cell.passableDirs[(int)OrthoDir.North] = true;
                        }
                        if (e.type == CellInfo.CellType.Stairway) {
                            e.stairAnchoredLow = true;
                            e.passableDirs[(int)OrthoDir.West] = true;
                            cell.passableDirs[(int)OrthoDir.East] = true;
                        }
                        if (s.type == CellInfo.CellType.Stairway) {
                            s.passableDirs[(int)OrthoDir.North] = true;
                            cell.passableDirs[(int)OrthoDir.South] = true;
                        }
                        if (w.type == CellInfo.CellType.Stairway) {
                            w.passableDirs[(int)OrthoDir.East] = true;
                            cell.passableDirs[(int)OrthoDir.West] = true;
                        }
                    }
                }
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

    public bool Flip() {
        return Random.Range(0.0f, 1.0f) > 0.5f;
    }

    private int RandomHallSize() {
        //int h;
        //float r = Random.Range(0.0f, 1.0f);
        //if      (r < 0.5)   h = 2;
        //else if (r < 0.75)  h = 3;
        //else if (r < 0.95)  h = 1;
        //else                h = 5;
        //return h;
        return 1;
    }

    private int RandomRoomSize() {
        // we're standardizing rooms at 7*7 so as to be able to fill them with hand stuff
        return 7;
    }

    private int RandomNewZ(int oldZ) {
        //int z;
        //float r = Random.Range(0.0f, 1.0f);
        //if      (r < 0.3)   z = oldZ;
        //else if (r < 0.5)   z = oldZ + 2;
        //else if (r < 0.95)  z = oldZ + 4;
        //else                z = oldZ + 6;
        //return z;
        return oldZ + 2;
    }
}
