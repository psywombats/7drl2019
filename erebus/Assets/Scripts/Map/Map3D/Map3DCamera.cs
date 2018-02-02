using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map3DCamera : MonoBehaviour {

    public bool FixedZ;

    private float lastFixedZ;

    public void Awake() {
        lastFixedZ = transform.position.z;
    }

    public void Update() {
        if (FixedZ) {
            transform.position = new Vector3(transform.position.x, transform.position.y, lastFixedZ);
        }
    }
}
