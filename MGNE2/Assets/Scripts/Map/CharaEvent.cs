using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

[RequireComponent(typeof(MapEvent))]
public class CharaEvent : MonoBehaviour {

    public static readonly string FaceEvent = "eventFace";

    // Editor
    public float PixelsPerSecond = 36.0f;

    // Public
    public MapEvent Event { get { return GetComponent<MapEvent>(); } }

    private OrthoDir facing;
    public OrthoDir Facing {
        get { return facing; }
        set {
            if (facing != value) {
                facing = value;
                Event.GetComponent<Dispatch>().Signal(FaceEvent, value);
            }
        }
    }

    public bool Tracking { get; private set; }

    // Private
    private Vector2 movementSlop;

    public void Start() {
        movementSlop = new Vector2(0.0f, 0.0f);
    }

    public void Update() {
        MapEvent mapEvent = GetComponent<MapEvent>();
        if (Tracking) {
            mapEvent.PositionPx = Vector2.MoveTowards((mapEvent.PositionPx + movementSlop), mapEvent.TargetPosition, PixelsPerSecond * Time.deltaTime);
            movementSlop.Set(mapEvent.PositionPx.x - (float)Mathf.Floor(mapEvent.PositionPx.x), mapEvent.PositionPx.y - (float)Mathf.Floor(mapEvent.PositionPx.y));
            mapEvent.PositionPx = mapEvent.PositionPx - movementSlop;
            Vector2 position2 = mapEvent.PositionPx;
            if (position2 == mapEvent.TargetPosition) {
                Tracking = false;
            }
        }
    }

    public void Step(OrthoDir dir) {
        if (!Tracking) {
            MapEvent mapEvent = GetComponent<MapEvent>();
            Tracking = true;
            mapEvent.Position += dir.XY();
            mapEvent.TargetPosition = mapEvent.PositionPx + Vector2.Scale(dir.PxXY(), Map.TileSizePx);
            Facing = OrthoDirExtensions.DirectionOfPx(mapEvent.TargetPosition - mapEvent.PositionPx);
        }
    }

    // checks if the given location is passable for this character
    public bool PassableAt(IntVector2 loc) {
        int thisLayerIndex;
        for (thisLayerIndex = 0; thisLayerIndex < Event.Parent.transform.childCount; thisLayerIndex += 1) {
            if (Event.Parent.transform.GetChild(thisLayerIndex).gameObject.GetComponent<ObjectLayer>() == Event.Layer) {
                break;
            }
        }

        for (int i = thisLayerIndex - 1; i >= 0 && i >= thisLayerIndex - 2; i -= 1) {
            TileLayer layer = Event.Parent.transform.GetChild(i).GetComponent<TileLayer>();
            if (loc.x < 0 || loc.x >= Event.Parent.Width || loc.y < 0 || loc.y >= Event.Parent.Height) {
                return false;
            }
            if (layer != null) {
                if (!Event.Parent.PassableAt(layer, loc)) {
                    return false;
                }
            }
        }

        return true;
    }
}
