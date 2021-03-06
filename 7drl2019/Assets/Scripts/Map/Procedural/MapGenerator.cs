﻿using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TacticsTerrainMesh))]
public class MapGenerator : MonoBehaviour {

    private float MaxHeightDelta = 1.5f;

    public Vector2Int sizeInRoomsMin;
    public Vector2Int sizeInRoomsMax;
    public int stairLength = 3;

    public Vector2Int entryRoomCoords = new Vector2Int(0, 0);
    public Vector2Int exitRoomCoords = new Vector2Int(4, 4);

    [HideInInspector] public Vector2Int sizeInCells { get; private set; }
    [HideInInspector] public Vector2Int sizeInRooms { get; private set; }
    [HideInInspector] public CellInfo[,] cells { get; private set; }
    [HideInInspector] public RoomInfo[,] rooms { get; private set; }
    [HideInInspector] public int[] widths { get; private set; }
    [HideInInspector] public int[] heights { get; private set; }

    [Space]
    public Tile defaultImpassTile;
    public ChestEvent chestEventPrefab;
    public MapEvent3D startEventPrefab;
    public MapEvent3D endEventPrefab;
    public MapEvent3D impassEventPrefab;
    public MapEvent3D targetPrefab;

    [Space]
    public GenerationTable table;
    public int level = 1;
    public int finalLevel = 10;
    public Map finalLevelPrefab;

    public bool startStairsSW, endStairsNW;

    public void GenerateMesh(MapGenerator lastMap = null) {
        TacticsTerrainMesh mesh = GetComponent<TacticsTerrainMesh>();

        // copy oldbie values if needed
        if (lastMap != null) {
            sizeInRoomsMin = lastMap.sizeInRoomsMin;
            sizeInRoomsMax = lastMap.sizeInRoomsMax;
            stairLength = lastMap.stairLength;
            startEventPrefab = lastMap.startEventPrefab;
            endEventPrefab = lastMap.endEventPrefab;
            impassEventPrefab = lastMap.impassEventPrefab;
            defaultImpassTile = lastMap.defaultImpassTile;
            chestEventPrefab = lastMap.chestEventPrefab;
            targetPrefab = lastMap.targetPrefab;
            finalLevel = lastMap.finalLevel;
            finalLevelPrefab = lastMap.finalLevelPrefab;
            table = lastMap.table;
            level = lastMap.level + 1;
        }

        if (level == finalLevel) {
            Map other = Instantiate(finalLevelPrefab);
            Global.Instance().Maps.activeMap = other;
            return;
        }
        
        // wipe what's already there
        
        mesh.ClearTiles();
        foreach (MapEvent toRemove in GetComponent<Map>().GetEvents<MapEvent>()) {
            if (toRemove.GetComponent<PCEvent>() == null) {
                if (toRemove.GetComponent<BattleEvent>() && Application.isPlaying) {
                    GetComponent<BattleController>().RemoveUnit(toRemove.GetComponent<BattleEvent>().unit);
                }
                GetComponent<Map>().RemoveEvent(toRemove, true);
            }
        }

        // work out some constants
        if (Flip()) {
            sizeInRooms = new Vector2Int(sizeInRoomsMin.x, sizeInRoomsMax.y);
        } else {
            sizeInRooms = new Vector2Int(sizeInRoomsMax.x, sizeInRoomsMin.y);
        }
        int seed =  (int)DateTime.Now.Ticks;
        Random.InitState(seed);
        Debug.Log("using seed " + seed);
        rooms = new RoomInfo[sizeInRooms.x, sizeInRooms.y];
        sizeInCells = sizeInRooms * 2 - new Vector2Int(1, 1);
        cells = new CellInfo[sizeInCells.x, sizeInCells.y];

        // are we going to add the starting stairwell on the left or right?
        if (lastMap != null) {
            if (lastMap.endStairsNW) {
                entryRoomCoords = new Vector2Int(Random.Range(0, 2), 0);
                startStairsSW = false;
            } else {
                entryRoomCoords = new Vector2Int(0, Random.Range(0, 2));
                startStairsSW = true;
            }
            exitRoomCoords = new Vector2Int(sizeInRooms.x - 1, sizeInRooms.y - 1);
            if (Flip()) {
                exitRoomCoords.x -= Random.Range(0, 2);
            } else {
                exitRoomCoords.y -= Random.Range(0, 2);
            }
        } else {
            if (entryRoomCoords == Vector2Int.zero) startStairsSW = Flip();
            else if (entryRoomCoords.y > 0) startStairsSW = true;
            else startStairsSW = false;
        }
        if (exitRoomCoords == new Vector2Int(sizeInRooms.x - 1, sizeInRooms.y - 1)) endStairsNW = Flip();
        else if (exitRoomCoords.x < sizeInRooms.x - 1) endStairsNW = true;
        else endStairsNW = false;

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
                    cells[x, y].startX = startStairsSW ? stairLength : 0;
                } else {
                    cells[x, y].startX = cells[x - 1, y].startX + widths[x - 1];
                }
                if (y == 0) {
                    cells[x, y].startY = startStairsSW ? 0 : stairLength;
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
                    float z;
                    if (x == 0 && y == 0) {
                        z = stairLength / 2.0f + 0.5f;
                    } else {
                        float oldZ;
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
            RandUtils.Shuffle(adjacents);
            RoomInfo adj = null;
            foreach (RoomInfo adj2 in adjacents) {
                if (!adj2.cell.connected) {
                    continue;
                }
                CellInfo connector = cells[(room.cell.x + adj2.cell.x) / 2, (room.cell.y + adj2.cell.y) / 2];
                if (Mathf.Abs(adj2.z - room.z) / (widths[connector.x] * heights[connector.y]) > 2) {
                    continue;
                }
                adj = adj2;
                break;
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
        int wallX = cornerRoom.cell.startX + cornerRoom.cell.sizeX + 1;
        int wallY = cornerRoom.cell.startY + cornerRoom.cell.sizeY + 1;
        mesh.Resize(new Vector2Int(
            wallX + (endStairsNW ? 0 : stairLength),
            wallY + (endStairsNW ? stairLength : 0)),
            0.0f);
        for (int x = startStairsSW ? stairLength : 0; x < wallX; x += 1) {
            mesh.SetHeight(x, wallY - 1, cornerRoom.z + MaxHeightDelta + 1);
            mesh.SetTile(x, wallY - 1, defaultImpassTile);
        }
        for (int y = startStairsSW ? 0 : stairLength; y < wallY; y += 1) {
            mesh.SetHeight(wallX - 1, y, cornerRoom.z + MaxHeightDelta + 1);
            mesh.SetTile(wallX - 1, y, defaultImpassTile);
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

        // render rooms
        for (int x = 0; x < sizeInRooms.x; x += 1) {
            for (int y = 0; y < sizeInRooms.y; y += 1) {
                rooms[x, y].FillWithVault(mesh);
            }
        }

        // add the starter stairs
        RoomInfo startRoom = rooms[entryRoomCoords.x, entryRoomCoords.y];
        int stairX, stairZ;
        if (startStairsSW) {
            stairX = startRoom.cell.startX - 1;
            stairZ = startRoom.cell.startY + (startRoom.cell.sizeY / 2);
        } else {
            stairX = startRoom.cell.startX + (startRoom.cell.sizeX / 2);
            stairZ = startRoom.cell.startY - 1;
        }
        float stairY = startRoom.z - 0.5f;
        for (int i = 0; i < stairLength; i += 1) {
            mesh.SetHeight(stairX, stairZ, stairY);
            MapEvent3D ev;
            if (i == 0) {
                ev = Instantiate(startEventPrefab);
            } else {
                ev = Instantiate(impassEventPrefab);
            }
            GetComponent<Map>().AddEvent(ev);
            ev.SetLocation(new Vector2Int(stairX, stairZ));
            stairY -= 0.5f;
            stairX += startStairsSW ? -1 : 0;
            stairZ += startStairsSW ? 0 : -1;
        }

        // add the end stairs
        RoomInfo endRoom = rooms[exitRoomCoords.x, exitRoomCoords.y];
        if (endStairsNW) {
            stairX = endRoom.cell.startX + (endRoom.cell.sizeX / 2);
            stairZ = endRoom.cell.startY + endRoom.cell.sizeY;
        } else {
            stairX = endRoom.cell.startX + endRoom.cell.sizeX;
            stairZ = endRoom.cell.startY + (endRoom.cell.sizeY / 2);
        }
        stairY = endRoom.z + 0.5f;
        for (int i = 0; i < stairLength; i += 1) {
            MapEvent3D ev;
            if (i == 0) {
                ev = Instantiate(endEventPrefab);
            } else {
                ev = Instantiate(impassEventPrefab);
            }
            GetComponent<Map>().AddEvent(ev);
            ev.SetLocation(new Vector2Int(stairX, stairZ));
            mesh.SetHeight(stairX, stairZ, stairY);
            stairY += 0.5f;
            stairX += endStairsNW ? 0 : 1;
            stairZ += endStairsNW ? 1 : 0;
        }

        // add the zoom target
        MapEvent3D zoom = Instantiate(targetPrefab);
        GetComponent<Map>().AddEvent(zoom);
        zoom.SetLocation(new Vector2Int(mesh.size.x / 2, mesh.size.y / 2));

        // generate the encounters
        List<RoomInfo> validRooms = new List<RoomInfo>();
        for (int x = 0; x < sizeInRooms.x; x += 1) {
            for (int y = 0; y < sizeInRooms.y; y += 1) {
                if (x != entryRoomCoords.x || y != entryRoomCoords.y) {
                    validRooms.Add(rooms[x, y]);
                }
            }
        }
        List<Encounter> encounters = table.GenerateEncounters(level);
        foreach (Encounter encounter in encounters) {
            RoomInfo room = validRooms[Random.Range(0, validRooms.Count)];
            Vector2Int loc = new Vector2Int(
                Random.Range(room.cell.startX, room.cell.startX + room.cell.sizeX),
                Random.Range(room.cell.startY, room.cell.startY + room.cell.sizeY));
            encounter.PlaceAt(GetComponent<Map>(), loc);
        }

        // generate the chests
        List<Item> items = table.GenerateItems(level);
        foreach (Item item in items) {
            ChestEvent chest = Instantiate(chestEventPrefab);
            GetComponent<Map>().AddEvent(chest.GetComponent<MapEvent>());
            Vector2Int loc;
            do {
                RoomInfo room = validRooms[Random.Range(0, validRooms.Count)];
                loc = new Vector2Int(
                    Random.Range(room.cell.startX + 1, room.cell.startX + room.cell.sizeX - 1),
                    Random.Range(room.cell.startY + 1, room.cell.startY + room.cell.sizeY - 1));
            } while (!chest.GetComponent<MapEvent>().CanPassAt(loc));
            chest.PopulateAndPlace(item, loc);
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

    private float RandomNewZ(float oldZ) {
        float z;
        float r = Random.Range(0.0f, 1.0f);
        if (r < 0.2)        z = oldZ;
        else if (r < 0.4)   z = oldZ + 2;
        else if (r < 0.75)  z = oldZ + 3;
        else                z = oldZ + 4;
        return z;
    }
}
