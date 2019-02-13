using UnityEngine;

public class MapEvent3D : MapEvent {

    public static Vector3 TileToWorldCoords(IntVector2 position) {
        return new Vector3(position.x, 0.0f, -1.0f * position.y);
    }

    public override Vector3 CalculateOffsetPositionPx(OrthoDir dir) {
        return positionPx + dir.Px3D();
    }

    public override void SetScreenPositionToMatchTilePosition() {
        // this is not correct for all OrthoDir 3DPX setups
        float y = transform.localPosition.y;
        transform.localPosition = TileToWorldCoords(position);
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
    }

    public override Vector3 InternalPositionToDisplayPosition(Vector3 position) {
        return position;
    }

    protected override void SetDepth() {
        // our global height is identical to the height of the parent layer
        transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0.0f, gameObject.transform.localPosition.z);
    }
}
