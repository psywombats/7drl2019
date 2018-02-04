using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;
using System;

/**
 * For our purposes, a CharaEvent is anything that's going to be moving around the map
 * or has a physical appearance. For parallel process or whatevers, they won't have this.
 */
[RequireComponent(typeof(MapEvent))]
public class CharaEvent : MonoBehaviour {

    public static readonly string FaceEvent = "eventFace";

    private static readonly string PropertySprite = "sprite";
    private static readonly string PropertyFacing = "face";

    // Editor
    public float TilesPerSecond = 2.0f;
    public OrthoDir InitialFacing;

    // Public
    public Map Parent { get { return GetComponent<MapEvent>().Parent; } }
    public ObjectLayer Layer { get { return GetComponent<MapEvent>().Layer; } }
    public Vector3 TargetPositionPx { get; set; }

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

    public void Start() {
        Facing = InitialFacing;
    }

    public void Populate(IDictionary<string, string> properties) {
        if (properties.ContainsKey(PropertyFacing)) {
            InitialFacing = OrthoDirExtensions.Parse(properties[PropertyFacing]);
            Facing = InitialFacing;
        }
        if (properties.ContainsKey(PropertySprite)) {
            gameObject.AddComponent<CharaAnimator>().Populate(properties[PropertySprite]);
        }
        GetComponent<MapEvent>().Passable = false;
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

    public IEnumerator StepRoutine(OrthoDir dir) {
        if (Tracking) {
            yield break;
        }
        Tracking = true;

        MapEvent mapEvent = GetComponent<MapEvent>();
        mapEvent.Position += dir.XY();
        TargetPositionPx = mapEvent.CalculateOffsetPositionPx(dir);
        Facing = dir;

        while (true) {
            mapEvent.PositionPx = Vector3.MoveTowards(mapEvent.PositionPx, TargetPositionPx, TilesPerSecond * Time.deltaTime);

            // I think we actually want to handle this via prefabs now
            if (Global.Instance().Maps.Camera.Target == GetComponent<MapEvent>()) {
                Global.Instance().Maps.Camera.ManualUpdate();
            }

            if (mapEvent.PositionPx == TargetPositionPx) {
                Tracking = false;
                break;
            } else {
                yield return null;
            }
        }
    }

    public IEnumerator StepMultiRoutine(OrthoDir dir, int count) {
        for (int i = 0; i < count; i += 1) {
            yield return StartCoroutine(StepRoutine(dir));
        }
    }

    public IEnumerator PathToRoutine(IntVector2 location) {
        List<IntVector2> path = Parent.FindPath(this, location);
        if (path == null) {
            yield break;
        }
        foreach (IntVector2 target in path) {
            yield return StartCoroutine(StepRoutine(OrthoDirExtensions.DirectionOf(target - GetComponent<MapEvent>().Position)));
        }
    }
}
