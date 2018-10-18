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

    public void Start() {
        WarpToTarget();
    }

    public void Update() {
        CopyTargetPosition();
        transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                ScreenPosForTile(targetTileLocation),
                ref velocity,
                snapTime);
    }

    public void WarpToTarget() {
        transform.localPosition = ScreenPosForTile(targetTileLocation);
    }

    // TODO: this makes some very bad assumptions
    private Vector3 ScreenPosForTile(IntVector2 tilePos) {
        return new Vector3(tilePos.x, transform.localPosition.y, -1.0f * tilePos.y);
    }

    private void CopyTargetPosition() {
        if (Target != null) {
            targetTileLocation = Target.Position;
        }
    }
}
