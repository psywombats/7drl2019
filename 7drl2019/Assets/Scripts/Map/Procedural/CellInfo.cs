using UnityEngine;
using System.Collections;

public struct CellInfo {

    public enum CellType {
        Room,
        Hall,
        Pillar,
    }

    public int x, y;                // in cellspace
    public int startX, startY;      // in tilespace
    public int sizeX, sizeY;
    public CellType type;

    public void RenderTerrain(MapGenerator gen, TacticsTerrainMesh mesh) {
        switch (type) {
            case CellType.Hall:
                RenderHall(gen, mesh);
                break;
            case CellType.Pillar:
                RenderPillar(gen, mesh);
                break;
            case CellType.Room:
                RenderRoom(gen, mesh);
                break;
        }
    }

    private void RenderRoom(MapGenerator gen, TacticsTerrainMesh mesh) {
        RoomInfo room = gen.rooms[x / 2, y / 2];
        for (int y = startY; y < startY + sizeY; y += 1) {
            for (int x = startX; x < startX + sizeX; x += 1) {
                mesh.SetHeight(x, y, room.z / 2.0f);
            }
        }
    }

    private void RenderHall(MapGenerator gen, TacticsTerrainMesh mesh) {
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
        int deltaZ = highRoom.z - lowRoom.z;

        for (int y = 0; y < sizeY; y += 1) {
            for (int x = 0; x < sizeX; x += 1) {
                float t;
                if (sizeX > sizeY) {
                    t = ((float)x / sizeX);
                } else {
                    t = ((float)y / sizeY);
                }
                mesh.SetHeight(startX + x, startY + y, (lowRoom.z + Mathf.RoundToInt(t * deltaZ)) / 2.0f);
            }
        }
    }

    private void RenderPillar(MapGenerator gen, TacticsTerrainMesh mesh) {
        float maxZ = 0;
        foreach (Vector2Int toCheck in new Vector2Int[] {
            new Vector2Int(startX - 1, startY),
            new Vector2Int(startX, startY - 1),
            new Vector2Int(startX, startY + 1),
            new Vector2Int(startX + 1, startY),
        }) {
            float h = mesh.HeightAt(toCheck);
            if (h > maxZ) {
                maxZ = h;
            }
        }
        for (int y = startY; y < startY + sizeY; y += 1) {
            for (int x = startX; x < startX + sizeX; x += 1) {
                mesh.SetHeight(x, y, maxZ);
            }
        }
    }
}
