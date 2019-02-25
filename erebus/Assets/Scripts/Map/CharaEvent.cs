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

    private static readonly float Gravity = -20.0f;
    private static readonly float JumpHeightUpMult = 1.2f;
    private static readonly float JumpHeightDownMult = 1.6f;

    public static readonly string FaceEvent = "eventFace";

    public OrthoDir initialFacing = OrthoDir.South;
    public GameObject doll;

    public MapEvent parent { get { return GetComponent<MapEvent>(); } }
    public Map map { get { return parent.parent; } }

    private Vector3 targetPx;

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

    public void FaceToward(MapEvent other) {
        facing = GetComponent<MapEvent>().DirectionTo(other);
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
    public bool CanPassAt(Vector2Int loc) {
        if (!GetComponent<MapEvent>().switchEnabled) {
            return true;
        }

        foreach (MapEvent mapEvent in map.GetEventsAt(loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }

        return GetComponent<MapEvent>().CanPassAt(loc);
    }

    public IEnumerator PathToRoutine(Vector2Int location) {
        List<Vector2Int> path = map.FindPath(GetComponent<MapEvent>(), location);
        if (path == null) {
            yield break;
        }
        MapEvent mapEvent = GetComponent<MapEvent>();
        foreach (Vector2Int target in path) {
            OrthoDir dir = mapEvent.DirectionTo(target);
            yield return StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
        }
    }

    public IEnumerator StepRoutine(OrthoDir dir) {
        facing = dir;
        Vector2Int offset = parent.OffsetForTiles(dir);
        Vector3 startPx = parent.positionPx;
        targetPx = parent.TileToWorldCoords(parent.position);
        if (targetPx.y == startPx.y) {
            yield return parent.LinearStepRoutine(dir);
        } else if (targetPx.y > startPx.y) {
            // jump up routine routine
            float duration = (targetPx - startPx).magnitude / parent.CalcTilesPerSecond() / 2.0f * JumpHeightUpMult;
            yield return JumpRoutine(startPx, targetPx, duration);
            yield return CoUtils.Wait(1.0f / parent.CalcTilesPerSecond() / 2.0f);
        } else {
            // jump down routine
            float elapsed = 0.0f;
            float walkRatio = 0.65f;
            float walkDuration = walkRatio / parent.CalcTilesPerSecond();
            while (true) {
                float t = elapsed / walkDuration;
                elapsed += Time.deltaTime;
                parent.transform.position = new Vector3(
                    startPx.x + t * (targetPx.x - startPx.x) * walkRatio,
                    startPx.y,
                    startPx.z + t * (targetPx.z - startPx.z) * walkRatio);
                if (elapsed >= walkDuration) {
                    break;
                }
                yield return null;
            }
            float dy = targetPx.y - startPx.y;
            float jumpDuration = Mathf.Sqrt(dy / Gravity) * JumpHeightDownMult;
            yield return JumpRoutine(parent.transform.position, targetPx, jumpDuration);
            if (dy <= -1.0f) {
                yield return CoUtils.Wait(JumpHeightDownMult / parent.CalcTilesPerSecond() / 2.0f);
            }
        }
    }

    private IEnumerator JumpRoutine(Vector3 startPx, Vector3 targetPx, float duration) {
        float elapsed = 0.0f;
        
        float dy = (targetPx.y - startPx.y);
        float b = (dy - Gravity * (duration * duration)) / duration;
        while (true) {
            float t = elapsed / duration;
            elapsed += Time.deltaTime;
            parent.transform.position = new Vector3(
                startPx.x + t * (targetPx.x - startPx.x),
                startPx.y + Gravity * (elapsed * elapsed) + b * elapsed,
                startPx.z + t * (targetPx.z - startPx.z));
            if (elapsed >= duration) {
                break;
            }
            yield return null;
        }
        parent.SetScreenPositionToMatchTilePosition();
    }
}
