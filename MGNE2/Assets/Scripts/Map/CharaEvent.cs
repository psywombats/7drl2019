using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
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

    public bool Tracking;

    public MapEvent Event { get { return GetComponent<MapEvent>(); } }

    public void Update() {
        MapEvent mapEvent = GetComponent<MapEvent>();
        if (Tracking) {
            mapEvent.PositionPx = Vector2.MoveTowards(mapEvent.PositionPx, mapEvent.TargetPosition, PixelsPerSecond * Time.deltaTime);
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
            if (layer != null) {
                if (!Event.Parent.PassableAt(layer, loc)) {
                    return false;
                }
            }
        }

        return true;
    }
}
