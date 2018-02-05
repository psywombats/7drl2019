using UnityEngine;
using System.Collections;

public class OscillationComponent : MonoBehaviour {

    public enum OscillationOffsetMode {
        StartsAtMiddle,
        StartsAtExtreme
    };

    public enum OscillationMovementMode {
        Sinusoidal,
        Linear
    };

    public float durationSeconds = 1.0f;
    public Vector3 maxOffset;
    public OscillationMovementMode movementMode = OscillationMovementMode.Sinusoidal;
    public OscillationOffsetMode offsetMode = OscillationOffsetMode.StartsAtMiddle;

    private Vector3 originalPosition;
    private float elapsed;

    public void Start() {
        originalPosition = gameObject.transform.localPosition;
        Reset();
    }

    public void OnEnable() {
        Reset();
    }

    public void Update() {
        elapsed += Time.deltaTime;
        while (elapsed >= durationSeconds) {
            elapsed -= durationSeconds;
        }

        float completed = (elapsed / durationSeconds);
        if (offsetMode == OscillationOffsetMode.StartsAtMiddle) {
            completed += 0.5f;
            if (completed > 1.0f) {
                completed -= 1.0f;
            }
        }

        float vectorMultiplier;
        if (movementMode == OscillationMovementMode.Sinusoidal) {
            vectorMultiplier = Mathf.Sin(completed * 2.0f * Mathf.PI);
        } else {
            vectorMultiplier = (completed * 2.0f) - 1.0f;
            if (vectorMultiplier < -0.5f) {
                vectorMultiplier = (vectorMultiplier * -1) - 1.0f;
            }
            if (vectorMultiplier > 0.5f) {
                vectorMultiplier = (vectorMultiplier * -1) + 1.0f;
            }
            vectorMultiplier *= 2.0f;
        }
        gameObject.transform.localPosition = originalPosition + maxOffset * vectorMultiplier;
    }

    private void Reset() {
        elapsed = 0;
    }
}
