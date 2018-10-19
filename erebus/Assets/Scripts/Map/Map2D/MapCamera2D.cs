using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera2D : MapCamera {

    public override void ManualUpdate() {
        base.ManualUpdate();
        Vector3 targetPos = target.transform.position;
        Vector3 oldPos = GetComponent<Camera>().transform.position;
        Vector3 newPos = new Vector3(targetPos.x + Map.TileSizePx / 2 * OrthoDir.North.Y(), 
            targetPos.y + Map.TileSizePx / 2 * OrthoDir.East.X(), 
            oldPos.z);
        GetComponent<Camera>().transform.position = newPos;
    }
}
