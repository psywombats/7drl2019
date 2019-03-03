using UnityEngine;
using System.Collections.Generic;
using System;

public class CellInfo {

    public enum CellType {
        Room,
        Hall,
        Stairway,
        Pillar,
        Switchback,
    }

    public CellType type;
    public int x, y;                // in cellspace
    public int startX, startY;      // in tilespace
    public int sizeX, sizeY;        // in tilespace

    public bool connected;

    public bool stairAnchoredLow, stairAnchoredLow2, stairAnchoredHigh;
    public float pillarZ;

    public CellInfo(int x, int y, int sizeX, int sizeY) {
        this.x = x;
        this.y = y;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
    }

    public void RenderTerrain(MapGenerator gen, TacticsTerrainMesh mesh) {
        switch (type) {
            case CellType.Hall:
                RenderHall(gen, mesh);
                break;
            case CellType.Stairway:
                RenderStairway(gen, mesh);
                break;
            case CellType.Switchback:
                RenderSwitchback(gen, mesh);
                break;
            case CellType.Pillar:
                RenderPillar(gen, mesh);
                break;
            case CellType.Room:
                RenderRoom(gen, mesh);
                break;
        }
    }

    public List<RoomInfo> AdjacentRooms(MapGenerator gen) {
        List<RoomInfo> results = new List<RoomInfo>();
        switch (type) {
            case CellType.Room:
                if (x > 0) results.Add(gen.rooms[x / 2 - 1, y / 2]);
                if (y > 0) results.Add(gen.rooms[x / 2, y / 2 - 1]);
                if (x < gen.sizeInCells.x - 1) results.Add(gen.rooms[x / 2 + 1, y / 2]);
                if (y < gen.sizeInCells.y - 1) results.Add(gen.rooms[x / 2, y / 2 + 1]);
                break;
            case CellType.Switchback:
            case CellType.Stairway:
            case CellType.Hall:
                if (x % 2 == 1) {
                    results.Add(gen.rooms[(x - 1) / 2, y / 2]);
                    results.Add(gen.rooms[(x + 1) / 2, y / 2]);
                } else {
                    results.Add(gen.rooms[x / 2, (y - 1) / 2]);
                    results.Add(gen.rooms[x / 2, (y + 1) / 2]);
                }
                break;
            case CellType.Pillar:
                break;
        }
        return results;
    }

    public OrthoDir DirTo(CellInfo cell) {
        return OrthoDirExtensions.DirectionOf3D(new Vector2Int(cell.x - x, cell.y - y));
    }

    private void RenderRoom(MapGenerator gen, TacticsTerrainMesh mesh) {
        RoomInfo room = gen.rooms[x / 2, y / 2];
        for (int y = startY; y < startY + sizeY; y += 1) {
            for (int x = startX; x < startX + sizeX; x += 1) {
                mesh.SetHeight(x, y, room.z);
            }
        }
    }

    private void RenderHall(MapGenerator gen, TacticsTerrainMesh mesh) {
        List<RoomInfo> rooms = AdjacentRooms(gen);
        float z = rooms[0].z < rooms[1].z ? rooms[0].z : rooms[1].z;
        RoomInfo room = gen.rooms[x / 2, y / 2];
        for (int y = startY; y < startY + sizeY; y += 1) {
            for (int x = startX; x < startX + sizeX; x += 1) {
                mesh.SetHeight(x, y, z);
            }
        }
    }

    private void RenderSwitchback(MapGenerator gen, TacticsTerrainMesh mesh) {
        RoomInfo lowRoom, highRoom;
        if (this.x % 2 == 1) {
            lowRoom = gen.rooms[(this.x - 1) / 2, this.y / 2];
            highRoom = gen.rooms[(this.x + 1) / 2, this.y / 2];
        } else {
            lowRoom = gen.rooms[this.x / 2, (this.y - 1) / 2];
            highRoom = gen.rooms[this.x / 2, (this.y + 1) / 2];
        }
        if (lowRoom.z > highRoom.z) {
            RoomInfo temp = lowRoom;
            lowRoom = highRoom;
            highRoom = temp;
        }
        float deltaZ = highRoom.z - lowRoom.z;
        
        float d = (float)deltaZ / (sizeX * sizeY);
        bool switched = false;
        int x = 0;
        int y = 0;
        for (int i = 0; i < sizeX * sizeY; i += 1) {
            mesh.SetHeight(startX + x, startY + y, lowRoom.z + Mathf.RoundToInt(d * i * 2.0f) / 2.0f);
            if (this.x % 2 == 1) {
                y += switched ? -1 : 1;
                if (y >= sizeY || y < 0) {
                    switched = !switched;
                    x += 1;
                    y = Math.Min(Math.Max(y, 0), sizeY - 1);
                }
            } else {
                x += switched ? -1 : 1;
                if (x >= sizeX || x < 0) {
                    switched = !switched;
                    y += 1;
                    x = Math.Min(Math.Max(x, 0), sizeX - 1);
                }
            }
        }
    }

        private void RenderStairway(MapGenerator gen, TacticsTerrainMesh mesh) {
        RoomInfo lowRoom, highRoom;
        if (x % 2 == 1) {
            lowRoom = gen.rooms[(x - 1) / 2, y / 2];
            highRoom = gen.rooms[(x + 1) / 2, y / 2];
        } else {
            lowRoom = gen.rooms[x / 2, (y - 1) / 2];
            highRoom = gen.rooms[x / 2, (y + 1) / 2];
        }
        if (lowRoom.z > highRoom.z) {
            RoomInfo temp = lowRoom;
            lowRoom = highRoom;
            highRoom = temp;
        }
        float deltaZ = highRoom.z - lowRoom.z;

        int w = 3;
        if (!stairAnchoredHigh && !stairAnchoredLow &&
                ((x % 2 == 1 && sizeX >= deltaZ * 2) || (y % 2 == 1 && sizeY >= deltaZ * 2))) {
            // we're eligible for a straight staircase
            for (int y = 0; y < sizeY; y += 1) {
                for (int x = 0; x < sizeX; x += 1) {
                    mesh.SetHeight(startX + x, startY + y, lowRoom.z);
                }
            }
            for (float i = 0.0f; i < deltaZ; i += 0.5f) {
                for (int j = 0; j < w; j += 1) {
                    if (x % 2 == 1) {
                        mesh.SetHeight(
                            startX + sizeX - (int)(i * 2),
                            startY + j + (sizeY - w) / 2,
                            lowRoom.z + deltaZ - i);
                    } else {
                        mesh.SetHeight(
                            startX + j + (sizeX - w) / 2,
                            startY + sizeY - (int)(i * 2),
                            lowRoom.z + deltaZ - i);
                    }
                }
            }
        } else {
            // a normal side staircase
            bool heightSwap = gen.Flip();
            for (int y = 0; y < sizeY; y += 1) {
                for (int x = 0; x < sizeX; x += 1) {
                    if (stairAnchoredLow && stairAnchoredHigh) {
                        // stretch across the entire room
                        heightSwap = false;
                        float t;
                        if (this.x % 2 == 1) {
                            t = (float)x / sizeX;
                        } else {
                            t = (float)y / sizeY;
                        }
                        mesh.SetHeight(startX + x, startY + y, lowRoom.z + Mathf.RoundToInt(t * deltaZ));
                    } else {
                        float h;
                        if (stairAnchoredLow) {
                            heightSwap = false;
                            if (this.x % 2 == 1) {
                                h = Mathf.Min(highRoom.z, lowRoom.z + (y + 1) * 0.5f);
                            } else {
                                h = Mathf.Min(highRoom.z, lowRoom.z + (x + 1) * 0.5f);
                            }
                        } else if (stairAnchoredHigh) {
                            heightSwap = false;
                            if (this.x % 2 == 1) {
                                h = Mathf.Max(lowRoom.z, highRoom.z - (y + 1) * 0.5f);
                            } else {
                                h = Mathf.Max(lowRoom.z, highRoom.z - (x + 1) * 0.5f);
                            }
                        } else {
                            if (this.x % 2 == 1) {
                                h = Mathf.Max(lowRoom.z, Mathf.Min(highRoom.z, lowRoom.z + (deltaZ * 0.5f) + (y - sizeY/2) * 0.5f));
                            } else {
                                h = Mathf.Max(lowRoom.z, Mathf.Min(highRoom.z, lowRoom.z + (deltaZ * 0.5f) + (x - sizeX/2) * 0.5f));
                            }
                        }
                        mesh.SetHeight(startX + x, startY + y, heightSwap ? lowRoom.z + (highRoom.z - h) : h);
                    }
                }
            }
        }
    }

    private void RenderPillar(MapGenerator gen, TacticsTerrainMesh mesh) {
        for (int y = startY; y < startY + sizeY; y += 1) {
            for (int x = startX; x < startX + sizeX; x += 1) {
                mesh.SetHeight(x, y, pillarZ);
            }
        }
    }
}
