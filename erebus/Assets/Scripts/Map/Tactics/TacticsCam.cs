using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCam : MapCamera {

    public Camera cam;
    public Vector3 targetPosition;
    public float snapTime = 0.2f;
    public float angle = 35.0f;
    public float distance = 12.0f;
    public bool fixAngleAndDistance = false;

    private Vector3 velocity = new Vector3(0, 0, 0);

    private static TacticsCam instance;
    public static TacticsCam Instance() {
        if (instance == null) {
            instance = FindObjectOfType<TacticsCam>();
        }
        return instance;
    }

    public override void ManualUpdate() {
        base.ManualUpdate();
        CopyTargetPosition();
    }

    public override Camera GetCameraComponent() {
        return cam;
    }

    public void Start() {
        WarpToTarget();
        if (Application.isPlaying) {
            Global.Instance().Maps.SetCamera(this);
        }
    }

    public void Update() {
        CopyTargetPosition();
        transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetPosition,
                ref velocity,
                snapTime);
        if (fixAngleAndDistance) {
            SetPositionToMatchAngleDist();
        }
    }

    public void OnValidate() {
        if (fixAngleAndDistance) {
            SetPositionToMatchAngleDist();
        }
    }

    public void WarpToTarget() {
        transform.localPosition = targetPosition;
    }

    public IEnumerator SwitchToDuelCamRoutine() {
        yield return null;
    }

    private void CopyTargetPosition() {
        if (target != null) {
            targetPosition = MapEvent3D.TileToWorldCoords(target.Position);
        }
    }

    private Vector3 LookTargetForPosition(Vector3 pos) {
        // ugly, makes assumptions about map3d transform
        return new Vector3(pos.x + 0.5f, pos.y + 1.0f, pos.z - 0.5f);
    }

    private void SetAngleDistToMatchPosition() {
        Vector3 lookingAt = LookTargetForPosition(targetPosition);
        Vector2 lookingAt2d = new Vector2(lookingAt.x, lookingAt.z);
        Vector2 current2d = new Vector2(cam.transform.position.x, cam.transform.position.z);
        float dist = Vector2.Distance(current2d, lookingAt2d);
        this.angle = Mathf.Atan2(cam.transform.position.y - lookingAt.y, dist) / 2 / Mathf.PI * 360.0f;
        this.distance = Vector3.Distance(cam.transform.position, lookingAt);
    }

    private void SetPositionToMatchAngleDist() {
        Vector3 lookingAt = LookTargetForPosition(targetPosition);
        cam.transform.localPosition = new Vector3(0.0f,
            Mathf.Sin(angle / 360.0f * 2.0f * Mathf.PI) * distance,
            -1.0f * Mathf.Cos(angle / 360.0f * 2.0f * Mathf.PI) * distance);
        cam.transform.localEulerAngles = new Vector3(angle, 0.0f, 0.0f);
    }
}
