using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

public class MapEvent2D : MapEvent {

    public Vector2 PositionPx2D {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        private set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }

    public override Vector3 CalculateOffsetPositionPx(OrthoDir dir) {
        throw new System.NotImplementedException();
    }

    protected override void SetScreenPositionToMatchTilePosition() {
        // this is probably not correct with our new screenspace measurements
        Vector2 transform = new Vector2(Map.TileSizePx, Map.TileSizePx);
        if (OrthoDir.East.X() != OrthoDir.East.Px2DX()) {
            transform.x = transform.x * -1;
        }
        if (OrthoDir.North.Y() != OrthoDir.North.Px2DY()) {
            transform.y = transform.y * -1;
        }
        PositionPx2D = Vector2.Scale(Position, transform);
        if (OrthoDir.East.X() != OrthoDir.East.Px2DX()) {
            PositionPx2D = new Vector2(PositionPx.x - Map.TileSizePx, PositionPx.y);
        }
        if (OrthoDir.North.Y() != OrthoDir.North.Px2DY()) {
            PositionPx2D = new Vector2(PositionPx.x, PositionPx.y - Map.TileSizePx);
        }
    }

    protected override void SetDepth() {
        if (Parent != null) {
            for (int i = 0; i < Parent.transform.childCount; i += 1) {
                if (Layer == Parent.transform.GetChild(i).gameObject.GetComponent<ObjectLayer>()) {
                    float depthPerLayer = -1.0f;
                    float z = depthPerLayer * ((float)Position.y / (float)Parent.Height) + (depthPerLayer * (float)i);
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z);
                }
            }
        }
    }

    protected override void SetInitialLocation(RectangleObject rect) {
        if (rect != null) {
            Position.Set((int)rect.TmxPosition.x / Map.TileSizePx, (int)rect.TmxPosition.y / Map.TileSizePx);
        }
    }
}
