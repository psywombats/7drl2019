using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Map2DCamera : MonoBehaviour {

    public MapEvent Target;
    
    public void Update() {
        //ManualUpdate();
    }

    public void ManualUpdate() {
        Vector3 targetPos = Target.transform.position;
        Vector3 oldPos = GetComponent<Camera>().transform.position;
        Vector3 newPos = new Vector3(targetPos.x + Map.TileWidthPx / 2 * OrthoDir.North.Y(), targetPos.y + Map.TileHeightPx / 2 * OrthoDir.East.X(), oldPos.z);
        GetComponent<Camera>().transform.position = newPos;
    }
}
