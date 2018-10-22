using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour {

    public MapEvent target;

    // these are read by sprites, not actually enforced by the cameras
    public bool billboardX;
    public bool billboardY;

    public virtual void ManualUpdate() {

    }

    public virtual Camera GetCameraComponent() {
        return GetComponent<Camera>();
    }
}
