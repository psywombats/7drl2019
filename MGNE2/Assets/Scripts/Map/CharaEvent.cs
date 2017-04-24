using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;
using System;

[RequireComponent(typeof(MapEvent))]
public class CharaEvent : MonoBehaviour {

    public static readonly string FaceEvent = "eventFace";

    private static readonly string PropertySprite = "sprite";
    private static readonly string PropertyFacing = "face";

    // Editor
    public float PixelsPerSecond = 36.0f;
    public OrthoDir InitialFacing;

    // Public
    public Map Parent { get { return GetComponent<MapEvent>().Parent; } }
    public ObjectLayer Layer { get { return GetComponent<MapEvent>().Layer; } }
    public Vector2 TargetPosition { get; set; }

    private OrthoDir facing;
    public OrthoDir Facing {
        get { return facing; }
        set {
            if (facing != value) {
                facing = value;
                GetComponent<Dispatch>().Signal(FaceEvent, value);
            }
        }
    }

    public bool Tracking { get; private set; }

    // Private
    private Vector2 movementSlop;
    private Action onDestinationReached;

    public void Start() {
        movementSlop = new Vector2(0.0f, 0.0f);
        Facing = InitialFacing;
    }

    public void Update() {
        MapEvent mapEvent = GetComponent<MapEvent>();
        if (Tracking) {
            mapEvent.PositionPx = Vector2.MoveTowards((mapEvent.PositionPx + movementSlop), TargetPosition, PixelsPerSecond * Time.deltaTime);
            movementSlop.Set(mapEvent.PositionPx.x - (float)Mathf.Floor(mapEvent.PositionPx.x), mapEvent.PositionPx.y - (float)Mathf.Floor(mapEvent.PositionPx.y));
            mapEvent.PositionPx = mapEvent.PositionPx - movementSlop;
            Vector2 position2 = mapEvent.PositionPx;
            if (position2 == TargetPosition) {
                Tracking = false;
                if (onDestinationReached != null) {
                    Action toExecute = onDestinationReached;
                    onDestinationReached = null;
                    toExecute();
                }
            }
        }
    }

    public void Populate(IDictionary<string, string> properties) {
        if (properties.ContainsKey(PropertyFacing)) {
            InitialFacing = OrthoDirExtensions.Parse(properties[PropertyFacing]);
            Facing = InitialFacing;
        }
        if (properties.ContainsKey(PropertySprite)) {
            gameObject.AddComponent<CharaAnimator>().Populate(properties[PropertySprite]);
        }
    }

    public void Step(OrthoDir dir, Action onFinish = null) {
        if (!Tracking) {
            MapEvent mapEvent = GetComponent<MapEvent>();
            Tracking = true;
            mapEvent.Position += dir.XY();
            TargetPosition = mapEvent.PositionPx + Vector2.Scale(dir.PxXY(), Map.TileSizePx);
            Facing = OrthoDirExtensions.DirectionOfPx(TargetPosition - mapEvent.PositionPx);
            if (onFinish != null) {
                onDestinationReached = onFinish;
            }
        }
    }

    // the quintessential A* method
    public void PathTo(IntVector2 location, Action onFinish = null) {
        List<IntVector2> path = Parent.FindPath(this, location);
        if (path == null) {
            if (onFinish != null) {
                onFinish();
            }
        } else {
            StartCoroutine(CoUtils.RunWithCallback(PathRoutine(path), this, () => {
                if (onFinish != null) {
                    onFinish();
                }
            });
        }
    }

    // checks if the given location is passable for this character
    // takes into account both chip and event
    public bool CanPassAt(IntVector2 loc) {
        if (!GetComponent<MapEvent>().SwitchEnabled) {
            return true;
        }

        int thisLayerIndex = GetComponent<MapEvent>().LayerIndex;

        foreach (MapEvent mapEvent in Parent.GetEventsAt(Layer, loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }

        for (int i = thisLayerIndex - 1; i >= 0 && i >= thisLayerIndex - 2; i -= 1) {
            TileLayer layer = Parent.transform.GetChild(i).GetComponent<TileLayer>();
            if (loc.x < 0 || loc.x >= Parent.Width || loc.y < 0 || loc.y >= Parent.Height) {
                return false;
            }
            if (layer != null) {
                if (!Parent.IsChipPassableAt(layer, loc)) {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerator PathRoutine(List<IntVector2> path) {
        foreach (IntVector2 target in path) {
            bool pathing = true;
            Step(OrthoDirExtensions.DirectionOf(target - GetComponent<MapEvent>().Position), () => {
                pathing = false;
            });
            while (pathing) {
                yield return null;
            }
        }
    }
}
