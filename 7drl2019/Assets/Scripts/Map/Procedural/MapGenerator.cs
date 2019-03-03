using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

[RequireComponent(typeof(TacticsTerrainMesh))]
public class MapGenerator : MonoBehaviour {

    private int MaxHeightDelta = 2;

    public Vector2Int sizeInRooms;
    [HideInInspector] public Vector2Int sizeInCells { get; private set; }
    [HideInInspector] public CellInfo[,] cells { get; private set; }
    [HideInInspector] public RoomInfo[,] rooms { get; private set; }
    [HideInInspector] public int[] widths { get; private set; }
    [HideInInspector] public int[] heights { get; private set; }

    public void GenerateMesh() {
        TacticsTerrainMesh mesh = GetComponent<TacticsTerrainMesh>();
        mesh.ClearFacingTiles();

        int seed =  (int)DateTime.Now.Ticks;
        Random.InitState(seed);
        Debug.Log("using seed " + seed);
        
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
                        int oldZ;
                        if (y == 0) oldZ = rooms[x - 1, y].z;
                        else if (x == 0) oldZ = rooms[x, y - 1].z;
                        else oldZ = Math.Max(rooms[x, y - 1].z, rooms[x - 1, y].z);
                        z = RandomNewZ(oldZ);
                    }
                    rooms[x, y] = new RoomInfo(cells[x * 2, y * 2], z);
                }
                x -= 1;
                y += 1;
            }
        }

        // determine the initial passability map
        cells[0, 0].connected = true;
        UpdatePassability();

        // knock down walls until all rooms are accessible
        List<RoomInfo> roomsToGo = new List<RoomInfo>();
        for (int x = 0; x < sizeInRooms.x; x += 1) {
            for (int y = 0; y < sizeInRooms.y; y += 1) {
                roomsToGo.Add(rooms[x, y]);
            }
        }
        while (roomsToGo.Count > 0) {
            int index = Random.Range(0, roomsToGo.Count);
            RoomInfo room = roomsToGo[index];
            if (room.cell.connected) {
                roomsToGo.RemoveAt(index);
                continue;
            }
            List<RoomInfo> adjacents = room.cell.AdjacentRooms(this);
            Shuffle(adjacents);
            RoomInfo adj = null;
            foreach (RoomInfo adj2 in adjacents) {
                if (adj2.cell.connected) {
                    adj = adj2;
                    break;
                }
            }
            if (adj == null) {
                continue;
            }
            roomsToGo.RemoveAt(index);
            cells[(room.cell.x + adj.cell.x) / 2, (room.cell.y + adj.cell.y) / 2].connected = true;
            room.cell.connected = adj.cell.connected;

            if (adj.cell.connected) {
                UpdatePassability();
            }
        }

        // convert halls into stairways as needed
        for (int y = 0; y < sizeInCells.y; y += 1) {
            for (int x = 0; x < sizeInCells.x; x += 1) {
                CellInfo cell = cells[x, y];
                if (cell.type == CellInfo.CellType.Hall && cell.connected) {
                    List<RoomInfo> rooms = cell.AdjacentRooms(this);
                    float deltaZ = Math.Abs(rooms[0].z - rooms[1].z);
                    if (deltaZ > 1) {
                        if (deltaZ > cell.sizeX / 2 && deltaZ > cell.sizeY / 2) {
                            cell.type = CellInfo.CellType.Switchback;
                        } else {
                            cell.type = CellInfo.CellType.Stairway;
                        }
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
                        n.stairAnchoredHigh = true;
                    } else if (s.type == CellInfo.CellType.Switchback && s.sizeX > 1) {
                        cell.pillarZ = (s.AdjacentRooms(this)[0].z + s.AdjacentRooms(this)[1].z) / 2.0f;
                    } else if (w.type == CellInfo.CellType.Switchback && s.sizeY > 1) {
                        cell.pillarZ = (w.AdjacentRooms(this)[0].z + w.AdjacentRooms(this)[1].z) / 2.0f;
                    } else {
                        cell.pillarZ = rooms[(x - 1) / 2, (y - 1) / 2].z;
                    }
                }
            }
        }

        // decorator fringe
        RoomInfo cornerRoom = rooms[sizeInRooms.x - 1, sizeInRooms.y - 1];
        mesh.Resize(new Vector2Int(
            cornerRoom.cell.startX + cornerRoom.cell.sizeX + 1,
            cornerRoom.cell.startY + cornerRoom.cell.sizeY + 1));
        for (int x = 0; x < mesh.size.x; x += 1) {
            mesh.SetHeight(x, mesh.size.y - 1, cornerRoom.z + MaxHeightDelta + 1);
        }
        for (int y = 0; y < mesh.size.y; y += 1) {
            mesh.SetHeight(mesh.size.x - 1, y, cornerRoom.z + MaxHeightDelta + 1);
        }

        // render terrain
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

        mesh.Rebuild(true);
    }

    public bool Flip() {
        return Random.Range(0.0f, 1.0f) > 0.5f;
    }

    private void UpdatePassability() {
        bool dirty = true;

        while (dirty) {
            dirty = false;
            for (int y = 0; y < sizeInRooms.y; y += 1) {
                for (int x = 0; x < sizeInRooms.x; x += 1) {
                    RoomInfo room = rooms[x, y];
                    if (room.cell.connected) {
                        continue;
                    }
                    foreach (RoomInfo other in room.cell.AdjacentRooms(this)) {
                        CellInfo connector = cells[(room.cell.x + other.cell.x) / 2, (room.cell.y + other.cell.y) / 2];
                        if (other.cell.connected && (connector.connected || Math.Abs(room.z - other.z) <= MaxHeightDelta)) {
                            room.cell.connected = true;
                            connector.connected = true;
                            dirty = true;
                        }
                    }
                }
            }
        }
    }

    private static void Shuffle(List<RoomInfo> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n);
            RoomInfo value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private int RandomHallSize() {
        int h;
        float r = Random.Range(0.0f, 1.0f);
        if (r < 0.4) h = 2;
        else if (r < 0.70) h = 3;
        else if (r < 0.90) h = 1;
        else h = 5;
        return h;
    }

    private int RandomRoomSize() {
        // we're standardizing rooms at 7*7 so as to be able to fill them with hand stuff
        return 5;
    }

    private int RandomNewZ(int oldZ) {
        int z;
        float r = Random.Range(0.0f, 1.0f);
        if (r < 0.3)        z = oldZ;
        else if (r < 0.5)   z = oldZ + 2;
        else                z = oldZ + 4;
        return z;
    }
}
