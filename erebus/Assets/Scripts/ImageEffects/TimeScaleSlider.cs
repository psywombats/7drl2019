using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleSlider : MonoBehaviour {

    [Range(1.0f, 16.0f)]
    public float timeScale = 1.0f;

    public bool slowdown = true;

    public void Update() {
        float scale = timeScale;
        if (slowdown) {
            scale = 1.0f / scale;
        }
        Time.timeScale = scale;
    }
}
