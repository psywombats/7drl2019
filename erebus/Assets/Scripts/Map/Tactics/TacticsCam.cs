using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCam : MapCamera {

    private float DuelCamSnapTime = 0.5f;
    private float DuelCamDistance = 8.0f;

    public Camera cam;
    public Vector3 targetAngles;
    public float distance = 12.0f;

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
        targetDollyPosition = transform.localPosition;
        CopyTargetPosition();
        WarpToTarget();
        //if (Application.isPlaying) {
        //    Global.Instance().Maps.SetCamera(this);
        //}
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

    public void WarpToTarget() {
        transform.localPosition = targetDollyPosition;
        transform.localEulerAngles = targetDollyAngles;
        cam.transform.localPosition = targetCamPosition;
        cam.transform.localEulerAngles = targetCamAngles;
    }

    public void ResetToTacticsMode() {
        distance = standardDistance;
        targetAngles = standardAngles;
        CopyTargetPosition();
        WarpToTarget();
    }

    public void SetTargetLocation(IntVector2 loc) {
        target = null;
        targetDollyPosition = MapEvent3D.TileToWorldCoords(loc);
    }

    public IEnumerator SwitchToDuelCamRoutine(MapEvent target1, MapEvent target2) {
        Vector3 targetWorld1 = MapEvent3D.TileToWorldCoords(target1.position);
        Vector3 targetWorld2 = MapEvent3D.TileToWorldCoords(target2.position);
        float angle = Mathf.Atan2(targetWorld1.x - targetWorld2.x, targetWorld1.z - targetWorld2.z);
        angle = angle / 2.0f / Mathf.PI * 360.0f;
        angle += 90.0f;
        while (angle >= 180.0f) angle -= 180.0f;
        yield return SwitchToDuelCamRoutine((targetWorld1 + targetWorld2) / 2.0f, angle);
    }
    public IEnumerator SwitchToDuelCamRoutine(Vector3 centerPoint, float angle = 0.0f) {
        standardAngles = targetAngles;
        standardDistance = distance;

        snapTime = DuelCamSnapTime;
        targetAngles = new Vector3(5.0f, angle, 0.0f);
        target = null;
        targetDollyPosition = centerPoint;
        distance = DuelCamDistance;
        CopyTargetPosition();

        yield return new WaitForSeconds(DuelCamSnapTime);
    }

    public IEnumerator DuelZoomRoutine(float zoomDistance, float duration) {
        distance = distance - zoomDistance;
        snapTime = duration;
        CopyTargetPosition();
        yield return new WaitForSeconds(duration);
    }

    private void CopyTargetPosition() {
        if (target != null) {
            targetDollyPosition = MapEvent3D.TileToWorldCoords(target.position);
        }
        targetCamPosition = PositionForAngleDist();
        targetCamAngles = new Vector3(targetAngles.x, 0.0f, 0.0f);
        float angleY = targetAngles.y;
        while (angleY < 0.0f) angleY += 360.0f;
        targetDollyAngles = new Vector3(0.0f, angleY, 0.0f);
    }

    private Vector3 LookTargetForPosition(Vector3 pos) {
        // ugly, makes assumptions about map3d transform
        return new Vector3(pos.x + 0.5f, pos.y + 1.0f, pos.z - 0.5f);
    }

    private Vector3 PositionForAngleDist() {
        float angle = targetAngles.x;
        return new Vector3(0.0f,
                Mathf.Sin(angle / 360.0f * 2.0f * Mathf.PI) * distance,
                -1.0f * Mathf.Cos(angle / 360.0f * 2.0f * Mathf.PI) * distance);
    }
}
