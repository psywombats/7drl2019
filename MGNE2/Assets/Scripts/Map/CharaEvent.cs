using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapEvent))]
public class CharaEvent : MonoBehaviour {

    public static readonly string FaceEvent = "eventFace";

    public float PixelsPerSecond = 36.0f;

    private OrthoDir facing;
    public OrthoDir Facing {
        get { return facing; }
        set {
            if (facing != value) {
                facing = value;
                Event.EventDispatch.Signal(FaceEvent, value);
            }
        }
    }

    public MapEvent Event { get { return GetComponent<MapEvent>(); } }

    private bool tracking;

    public void Update() {
        MapEvent mapEvent = GetComponent<MapEvent>();
        if (tracking) {
            mapEvent.PositionPx = Vector2.MoveTowards(mapEvent.PositionPx, mapEvent.TargetPosition, PixelsPerSecond * Time.deltaTime);
            Vector2 position2 = mapEvent.PositionPx;
            if (position2 == mapEvent.TargetPosition) {
                tracking = false;
            }
        }
    }

    public void Step(OrthoDir dir) {
        if (!tracking) {
            MapEvent mapEvent = GetComponent<MapEvent>();
            tracking = true;
            mapEvent.Position += dir.XY();
            mapEvent.TargetPosition = mapEvent.PositionPx + Vector2.Scale(dir.PxXY(), Map.TileSizePx);
            Facing = OrthoDirExtensions.DirectionOfPx(mapEvent.TargetPosition - mapEvent.PositionPx);
        }
    }
}
