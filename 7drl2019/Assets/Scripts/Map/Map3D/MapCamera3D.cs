using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera3D : MapCamera {

    public bool FixedZ;

    private float lastFixedZ;

    public void Awake() {
        lastFixedZ = transform.position.z;
    }

    public void LateUpdate() {
        ManualUpdate();
    }

    public override void ManualUpdate() {
        base.ManualUpdate();
        if (FixedZ) {
            transform.position = new Vector3(transform.position.x, transform.position.y, lastFixedZ);
        }
    }
}
