using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

public class MapEvent2D : MapEvent {

    public Vector2 PositionPx {
        get { return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y); }
        private set { gameObject.transform.position = new Vector3(value.x, value.y, gameObject.transform.position.z); }
    }

    protected override void SetScreenPositionToMatchTilePosition() {
        Vector2 transform = Map.TileSizePx;
        if (OrthoDir.East.X() != OrthoDir.East.PxX()) {
            transform.x = transform.x * -1;
        }
        if (OrthoDir.North.Y() != OrthoDir.North.PxY()) {
            transform.y = transform.y * -1;
        }
        PositionPx = Vector2.Scale(Position, transform);
        if (OrthoDir.East.X() != OrthoDir.East.PxX()) {
            PositionPx = new Vector2(PositionPx.x - Map.TileWidthPx, PositionPx.y);
        }
        if (OrthoDir.North.Y() != OrthoDir.North.PxY()) {
            PositionPx = new Vector2(PositionPx.x, PositionPx.y - Map.TileHeightPx);
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
}
