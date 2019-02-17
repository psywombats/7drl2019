using UnityEngine;
using System.Collections;

public class OriginSpotter : MonoBehaviour {

    public void OnDrawGizmos() {
        Gizmos.DrawCube(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.5f, 0.5f, 0.5f));
    }
}
