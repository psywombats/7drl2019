using UnityEngine;
using System.Collections.Generic;

public class RoomInfo {

    public CellInfo cell;
    public int z;

    public bool connected;
    public bool[] passableDirs = new bool[4];

    public RoomInfo(CellInfo cell, int z) {
        this.cell = cell;
        this.z = z;
    }
}
