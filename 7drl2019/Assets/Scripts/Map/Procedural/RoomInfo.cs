using UnityEngine;
using System.Collections.Generic;

public class RoomInfo {

    public CellInfo cell;
    public float z;

    public RoomInfo(CellInfo cell, float z) {
        this.cell = cell;
        this.z = z;
    }

    public override string ToString() {
        return "[" + cell.x / 2 + "," + cell.y / 2 + "]";
    }
}
