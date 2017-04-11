using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCamera : MonoBehaviour {

    public MapEvent Target;
    
    public void Update() {
        if (Target != null) {
            StartCoroutine(CoUtils.RunAfterDelay(0.0f, () => {
                ManualUpdate();
            }));
        }
    }

    public void ManualUpdate() {
        Vector3 targetPos = Target.transform.position;
        Vector3 oldPos = GetComponent<Camera>().transform.position;
        Vector3 newPos = new Vector3(targetPos.x + Map.TileWidthPx / 2 * OrthoDir.North.Y(), targetPos.y + Map.TileHeightPx / 2 * OrthoDir.East.X(), oldPos.z);
        GetComponent<Camera>().transform.position = newPos;
    }
}
