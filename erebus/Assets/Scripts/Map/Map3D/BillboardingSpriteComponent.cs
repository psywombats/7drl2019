using UnityEngine;

/**
 * Renders the attached sprite as fixed-x at the camera.
 */
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class BillboardingSpriteComponent : MonoBehaviour {

    public bool billboardX = true;
    public bool billboardY;

    public void Update() {
        if (GetCamera() == null) {
            return;
        }
        if (billboardX || GetCamera().billboardX) {
            Vector3 angles = transform.eulerAngles;
            transform.eulerAngles = new Vector3(
                    GetCamera().GetCameraComponent().transform.eulerAngles.x,
                    angles.y,
                    angles.z);
        }
        if (billboardY || GetCamera().billboardY) {
            Vector3 angles = transform.eulerAngles;
            transform.eulerAngles = new Vector3(
                    angles.x,
                    GetCamera().GetCameraComponent().transform.eulerAngles.y,
                    angles.z);
        }
    }

    private MapCamera GetCamera() {
        if (Application.isPlaying) {
            return Global.Instance().Maps.camera;
        } else {
            return FindObjectOfType<MapCamera>();
        }
    }
}
