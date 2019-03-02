using UnityEngine;

public class MapCamera : MonoBehaviour {

    public Camera cam;
    public MapEvent3D initialTarget;
    public Vector3 targetAngles;
    public float targetDistance = 12.0f;

    [Space]
    // these are read by sprites, not actually enforced by the cameras
    public bool billboardX;
    public bool billboardY;
    
    public float snapTime { get; set; }

    private Vector3 targetCamPosition;
    private Vector3 targetCamAngles;
    private Vector3 targetDollyPosition;
    private Vector3 targetDollyAngles;
    private Vector3 dollyVelocity = new Vector3(0, 0, 0);
    private Vector3 dollyAngleVelocity = new Vector3(0, 0, 0);
    private Vector3 camVelocity = new Vector3(0, 0, 0);
    private Vector3 camAnglesVelocity = new Vector3(0, 0, 0);

    private float standardDistance;
    private Vector3 standardAngles;

    private MapEvent3D _target;
    public MapEvent3D target {
        get {
            return _target;
        }
        set {
            _target = value;
            CopyTargetPosition();
        }
    }
    
    public void Start() {
        target = initialTarget;
        targetDollyPosition = transform.localPosition;
        CopyTargetPosition();
        WarpToTarget();
    }

    public void Update() {
        if (target != null) {
            CopyTargetPosition();
        }
        transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDollyPosition,
                ref dollyAngleVelocity,
                snapTime);
        transform.localEulerAngles = Vector3.SmoothDamp(
                transform.localEulerAngles,
                targetDollyAngles,
                ref dollyVelocity,
                snapTime);
        cam.transform.localPosition = Vector3.SmoothDamp(
                cam.transform.localPosition,
                targetCamPosition,
                ref camVelocity,
                snapTime);
        cam.transform.localEulerAngles = Vector3.SmoothDamp(
                cam.transform.localEulerAngles,
                targetCamAngles,
                ref camAnglesVelocity,
                snapTime);
    }

    public void SetTargetLocation(Vector2 targetLocation, float height) {
        targetDollyPosition = new Vector3(targetLocation.x, height, targetLocation.y);
    }

    public void WarpToTarget(bool requiresRetarget = false) {
        if (target == null) {
            target = initialTarget;
        }
        if (requiresRetarget) {
            targetDollyPosition = transform.localPosition;
            CopyTargetPosition();
        }
        transform.localPosition = targetDollyPosition;
        transform.localEulerAngles = targetDollyAngles;
        cam.transform.localPosition = targetCamPosition;
        cam.transform.localEulerAngles = targetCamAngles;
    }

    private void CopyTargetPosition() {
        if (target != null) {
            targetDollyPosition = target.positionPx;
        }
        targetCamPosition = PositionForAngleDist();
        targetCamAngles = new Vector3(targetAngles.x, 0.0f, 0.0f);
        float angleY = targetAngles.y;
        while (angleY < 0.0f) angleY += 360.0f;
        targetDollyAngles = new Vector3(0.0f, angleY, 0.0f);
    }

    private Vector3 LookTargetForPosition(Vector3 pos) {
        // ugly, makes assumptions about map3d transform
        return new Vector3(pos.x + 0.5f, pos.y + 1.0f, pos.z + 0.5f);
    }

    private Vector3 PositionForAngleDist() {
        float angle = targetAngles.x;
        return new Vector3(0.0f,
                Mathf.Sin(angle / 360.0f * 2.0f * Mathf.PI) * targetDistance,
                -1.0f * Mathf.Cos(angle / 360.0f * 2.0f * Mathf.PI) * targetDistance);
    }
}
