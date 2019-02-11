using UnityEngine;

public class MapEvent2D : MapEvent {

    public Vector2 PositionPx2D {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        private set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }

    public override Vector3 CalculateOffsetPositionPx(OrthoDir dir) {
        return new Vector3(
            positionPx.x + dir.Px2DX(),
            positionPx.y + dir.Px2DY(),
            DepthForPosition(position + dir.XY()));
    }

    public override void SetScreenPositionToMatchTilePosition() {
        // this is maybe not correct with our new screenspace measurements
        Vector2 transform = new Vector2(1, 1);
        if (OrthoDir.East.X() != OrthoDir.East.Px2DX()) {
            transform.x = transform.x * -1;
        }
        if (OrthoDir.North.Y() != OrthoDir.North.Px2DY()) {
            transform.y = transform.y * -1;
        }
        PositionPx2D = Vector2.Scale(position, transform);
    }

    protected override void SetDepth() {
        if (parent != null) {
            float z = DepthForPosition(position);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z);
        }
    }

    private float DepthForPosition(IntVector2 position) {
        for (int i = 0; i < parent.transform.childCount; i += 1) {
            if (layer == parent.transform.GetChild(i).gameObject.GetComponent<ObjectLayer>()) {
                float depthPerLayer = -1.0f;
                return depthPerLayer * ((float)position.y / (float)parent.height) + (depthPerLayer * (float)i);
            }
        }
        return 0.0f;
    }
}
