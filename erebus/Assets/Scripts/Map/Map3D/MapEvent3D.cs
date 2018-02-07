using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

public class MapEvent3D : MapEvent {

    public override Vector3 CalculateOffsetPositionPx(OrthoDir dir) {
        return PositionPx + dir.Px3D();
    }

    protected override void SetScreenPositionToMatchTilePosition() {
        // this is not correct for all OrthoDir 3DPX setups
        transform.localPosition = new Vector3(Position.x, transform.localPosition.y, -1.0f * Position.y);
    }
    
    protected override void SetDepth() {
        // our global height is identical to the height of the parent layer
        transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0.0f, gameObject.transform.localPosition.z);
    }

    protected override void SetInitialLocation(RectangleObject rect) {
        if (rect != null) {
            Position.Set((int)rect.TmxPosition.x / Map.TileSizePx, (int)rect.TmxPosition.y / Map.TileSizePx);
        }
    }
}
