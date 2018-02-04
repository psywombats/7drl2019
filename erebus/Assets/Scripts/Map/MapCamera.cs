using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCamera : MonoBehaviour {

    public MapEvent Target;

    public virtual void ManualUpdate() {

    }
}
