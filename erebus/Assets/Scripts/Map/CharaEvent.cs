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
[DisallowMultipleComponent]
public class CharaEvent : MonoBehaviour {

    public static readonly string FaceEvent = "eventFace";

    private static readonly string PropertySprite = "sprite";
    private static readonly string PropertyFacing = "face";

    // Editor
    public float tilesPerSecond = 2.0f;
    public OrthoDir initialFacing;
    public GameObject doll;

    // Public
    public Map parent { get { return GetComponent<MapEvent>().Parent; } }
    public ObjectLayer layer { get { return GetComponent<MapEvent>().Layer; } }
    public Vector3 targetPositionPx { get; set; }

    private OrthoDir internalFacing;
    public OrthoDir facing {
        get {
            return internalFacing;
        }
        set {
            if (internalFacing != value) {
                internalFacing = value;
                GetComponent<Dispatch>().Signal(FaceEvent, value);
            }
        }
    }

    public bool tracking { get; private set; }

    public void Start() {
        facing = initialFacing;
    }

    public void Populate(IDictionary<string, string> properties) {
        if (properties.ContainsKey(PropertyFacing)) {
            initialFacing = OrthoDirExtensions.Parse(properties[PropertyFacing]);
            facing = initialFacing;
        }
        if (properties.ContainsKey(PropertySprite)) {
            if (GetComponent<MapEvent3D>() != null) {
                doll = new GameObject("Doll");
                doll.transform.parent = gameObject.transform;
                doll.transform.localPosition = new Vector3(0.25f, 0.0f, -1.0f);
                CharaAnimator animator = doll.AddComponent<CharaAnimator>();
                animator.ParentEvent = GetComponent<MapEvent>();
                animator.Populate(properties[PropertySprite]);
            } else {
                gameObject.AddComponent<CharaAnimator>().Populate(properties[PropertySprite]);
            }
            
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

        foreach (MapEvent mapEvent in parent.GetEventsAt(layer, loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }

        for (int i = thisLayerIndex - 1; i >= 0 && i >= thisLayerIndex - 2; i -= 1) {
            TileLayer layer = parent.transform.GetChild(i).GetComponent<TileLayer>();
            if (loc.x < 0 || loc.x >= parent.width || loc.y < 0 || loc.y >= parent.height) {
                return false;
            }
            if (layer != null) {
                if (!parent.IsChipPassableAt(layer, loc)) {
                    return false;
                }
            }
        }

        return true;
    }

    public IEnumerator StepRoutine(OrthoDir dir) {
        if (tracking) {
            yield break;
        }
        tracking = true;

        MapEvent mapEvent = GetComponent<MapEvent>();
        mapEvent.Position += dir.XY();
        targetPositionPx = mapEvent.CalculateOffsetPositionPx(dir);
        facing = dir;

        while (true) {
            mapEvent.PositionPx = Vector3.MoveTowards(mapEvent.PositionPx, targetPositionPx, tilesPerSecond * Time.deltaTime);

            // I think we actually want to handle this via prefabs now
            if (Global.Instance().Maps.Camera.Target == GetComponent<MapEvent>()) {
                Global.Instance().Maps.Camera.ManualUpdate();
            }

            if (mapEvent.PositionPx == targetPositionPx) {
                tracking = false;
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
        List<IntVector2> path = parent.FindPath(this, location);
        if (path == null) {
            yield break;
        }
        foreach (IntVector2 target in path) {
            yield return StartCoroutine(StepRoutine(OrthoDirExtensions.DirectionOf(target - GetComponent<MapEvent>().Position)));
        }
    }
}
