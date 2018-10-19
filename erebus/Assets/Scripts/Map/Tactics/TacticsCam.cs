using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsCam : MapCamera {

    public Camera cam;
    public float snapTime = 0.2f;
    public IntVector2 targetTileLocation;

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
        Global.Instance().Maps.SetCamera(this);
    }

    public void Update() {
        CopyTargetPosition();
        transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                MapEvent3D.TileToWorldCoords(targetTileLocation),
                ref velocity,
                snapTime);
    }

    public void WarpToTarget() {
        transform.localPosition = MapEvent3D.TileToWorldCoords(targetTileLocation);
    }

    private void CopyTargetPosition() {
        if (target != null) {
            targetTileLocation = target.Position;
        }
    }
}
