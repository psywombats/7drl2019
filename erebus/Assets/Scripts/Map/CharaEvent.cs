using System.Collections;
using System.Collections.Generic;
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

    // Editor
    public OrthoDir initialFacing = OrthoDir.South;
    public GameObject doll;

    // Public
    public Map parent { get { return GetComponent<MapEvent>().parent; } }

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

    private CharaAnimator _animator;
    public CharaAnimator animator {
        get {
            if (_animator == null) _animator = doll.GetComponent<CharaAnimator>();
            return _animator;
        }
    }

    public void Start() {
        facing = initialFacing;
        GetComponent<Dispatch>().RegisterListener(MapEvent.EventMove, (object payload) => {
            facing = (OrthoDir)payload;
        });
    }

    public void FaceToward(IntVector2 pos) {
        facing = OrthoDirExtensions.DirectionOf(pos - GetComponent<MapEvent>().position);
    }

    public void SetAppearance(string spriteKey) {
        animator.SetSpriteByKey(spriteKey);
    }

    // returns the sprite key currently in use
    public string GetAppearance() {
        return animator.spriteName;
    }

    // checks if the given location is passable for this character
    // takes into account both chip and event
    public bool CanPassAt(IntVector2 loc) {
        if (!GetComponent<MapEvent>().switchEnabled) {
            return true;
        }

        foreach (MapEvent mapEvent in parent.GetEventsAt(loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }

        return GetComponent<MapEvent>().CanPassAt(loc);
    }

    public IEnumerator PathToRoutine(IntVector2 location) {
        List<IntVector2> path = parent.FindPath(this, location);
        if (path == null) {
            yield break;
        }
        foreach (IntVector2 target in path) {
            OrthoDir dir = OrthoDirExtensions.DirectionOf(target - GetComponent<MapEvent>().position);
            yield return StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
        }
    }
}
