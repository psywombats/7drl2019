using UnityEngine;
using System.Collections;

public class SimpleRotator : MonoBehaviour {

    public float degreesPerSecond = 45.0f;

    public void Update() {
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y + degreesPerSecond * Time.deltaTime,
            transform.localEulerAngles.z
            );
    }
}
