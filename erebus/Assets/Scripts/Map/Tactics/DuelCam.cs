using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DuelCam : MonoBehaviour {

    private Vector3 targetPosition;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private float snapTime = 0.2f;

    private static DuelCam instance;
    public static DuelCam Instance() {
        if (instance == null) {
            instance = FindObjectOfType<DuelCam>();
        }
        return instance;
    }

    public void Start() {
        targetPosition = transform.position;
    }

    public void Update() {
        transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                snapTime);
    }

    public IEnumerator TransitionInZoomRoutine(float zoomDistance, float duration) {
        targetPosition = transform.position;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z - zoomDistance);
        snapTime = duration;
        yield return new WaitForSeconds(duration);
    }
}
