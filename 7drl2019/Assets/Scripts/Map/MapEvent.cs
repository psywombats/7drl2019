using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

/**
 * The generic "thing on the map" class for MGNE2. Usually comes from Tiled.
 */
[RequireComponent(typeof(Dispatch))]
[RequireComponent(typeof(LuaCutsceneContext))]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public abstract class MapEvent : MonoBehaviour {
    
    private const string PropertyCondition = "show";
    private const string PropertyInteract = "onInteract";
    private const string PropertyCollide = "onCollide";

    public const string EventEnabled = "enabled";
    public const string EventCollide = "collide";
    public const string EventInteract = "interact";
    public const string EventMove = "move";

    // Editor properties
    [HideInInspector] public Vector2Int location = new Vector2Int(0, 0);
    [HideInInspector] public Vector2Int size = new Vector2Int(1, 1);
    [Space]
    [Header("Movement")]
    public float tilesPerSecond = 2.0f;
    public bool passable = true;
    [Space]
    [Header("Lua scripting")]
    public string luaCondition;
    [TextArea(3, 6)] public string luaOnInteract;
    [TextArea(3, 6)] public string luaOnCollide;

    // Properties
    public LuaMapEvent luaObject { get; private set; }
    public Vector3 targetPositionPx { get; set; }
    public bool tracking { get; private set; }

    private Vector3 _internalPosition;
    public Vector3 positionPx {
        get { return _internalPosition; }
        set {
            _internalPosition = value;
            transform.localPosition = InternalPositionToDisplayPosition(_internalPosition);
        }
    }

    private Map _parent;
    public Map parent {
        get {
            // this is wiped in update but we'll cache it across frames anyway
            if (_parent != null) {
                return _parent;
            }
            GameObject parentObject = gameObject;
            while (parentObject.transform.parent != null) {
                parentObject = parentObject.transform.parent.gameObject;
                Map map = parentObject.GetComponent<Map>();
                if (map != null) {
                    _parent = map;
                    return map;
                }
            }
            return null;
        }
    }

    private ObjectLayer _layer;
    public ObjectLayer layer {
        get {
            if (_layer == null) {
                GameObject parent = gameObject;
                do {
                    parent = parent.transform.parent.gameObject;
                    ObjectLayer objLayer = parent.GetComponent<ObjectLayer>();
                    if (objLayer != null) {
                        _layer = objLayer;
                        break;
                    }
                } while (parent.transform.parent != null);
            }
            return _layer;
        }
    }

    private bool _switchEnabled = true;
    public bool switchEnabled {
        get {
            return _switchEnabled;
        }
        set {
            if (value != _switchEnabled) {
                GetComponent<Dispatch>().Signal(EventEnabled, value);
            }
            _switchEnabled = value;
        }
    }

    public abstract Vector3 TileToWorldCoords(Vector2Int location);

    // perform any pixel-perfect rounding needed for a pixel position
    public abstract Vector3 InternalPositionToDisplayPosition(Vector3 position);

    public EightDir DirectionTo(Vector2Int location) {
        return EightDirExtensions.DirectionOf(this.location - location);
    }

    public abstract float CalcTilesPerSecond();

    public void Awake() {
        luaObject = new LuaMapEvent(this);
    }

    public void Start() {
        if (Application.isPlaying) {
            luaObject.Set(PropertyCollide, luaOnCollide);
            luaObject.Set(PropertyInteract, luaOnInteract);
            luaObject.Set(PropertyCondition, luaCondition);

            positionPx = transform.localPosition;

            GetComponent<Dispatch>().RegisterListener(EventCollide, (object payload) => {
                OnCollide((AvatarEvent)payload);
            });
            GetComponent<Dispatch>().RegisterListener(EventInteract, (object payload) => {
                OnInteract((AvatarEvent)payload);
            });

            CheckEnabled();
        }
    }

    public virtual void Update() {
        if (Application.IsPlaying(this)) {
            CheckEnabled();
        }
    }

    public void OnDrawGizmos() {
        if (Selection.activeGameObject == gameObject) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.magenta;
        }
        DrawGizmoSelf();
    }

    public void CheckEnabled() {
        switchEnabled = luaObject.EvaluateBool(PropertyCondition, true);
    }

    public bool IsPassableBy(MapEvent other) {
        return passable || !switchEnabled;
    }

    public OrthoDir OrthoDirTo(MapEvent other) {
        return OrthoDirExtensions.DirectionOf3D(location - other.location);
    }

    public bool CanPassAt(Vector2Int loc) {
        if (!GetComponent<MapEvent>().switchEnabled) {
            return true;
        }
        if (loc.x < 0 || loc.x >= parent.width || loc.y < 0 || loc.y >= parent.height) {
            return false;
        }
        foreach (Tilemap layer in parent.layers) {
            if (layer.transform.position.z >= parent.objectLayer.transform.position.z && 
                    !parent.IsChipPassableAt(layer, loc)) {
                return false;
            }
        }
        foreach (MapEvent mapEvent in parent.GetEventsAt(loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }

        return true;
    }

    public IEnumerator PathToRoutine(Vector2Int location) {
        List<Vector2Int> path = parent.FindPath(this, location);
        if (path == null) {
            yield break;
        }
        MapEvent mapEvent = GetComponent<MapEvent>();
        foreach (Vector2Int target in path) {
            EightDir dir = mapEvent.DirectionTo(target);
            yield return StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
        }
    }

    public bool ContainsPosition(Vector2Int loc) {
        Vector2Int pos1 = location;
        Vector2Int pos2 = location + size;
        return loc.x >= pos1.x && loc.x < pos2.x && loc.y >= pos1.y && loc.y < pos2.y;
    }

    public void SetLocation(Vector2Int location) {
        this.location = location;
        SetScreenPositionToMatchTilePosition();
        SetDepth();
    }

    public void SetSize(Vector2Int size) {
        this.size = size;
        SetScreenPositionToMatchTilePosition();
        SetDepth();
    }

    // we have a solid TileX/TileY, please move the doll to the correct screen space
    public abstract void SetScreenPositionToMatchTilePosition();

    // set the one xyz coordinate not controlled by arrow keys
    public abstract void SetDepth();

    protected abstract void DrawGizmoSelf();

    // called when the avatar stumbles into us
    // before the step if impassable, after if passable
    private void OnCollide(AvatarEvent avatar) {
        luaObject.Run(PropertyCollide);
    }

    // called when the avatar stumbles into us
    // facing us if impassable, on top of us if passable
    private void OnInteract(AvatarEvent avatar) {
        if (GetComponent<CharaEvent>() != null) {
            GetComponent<CharaEvent>().facing = OrthoDirTo(avatar.GetComponent<MapEvent>());
        }
        luaObject.Run(PropertyInteract);
    }

    private LuaScript ParseScript(string lua) {
        if (lua == null || lua.Length == 0) {
            return null;
        } else {
            return new LuaScript(GetComponent<LuaContext>(), lua);
        }
    }

    private LuaCondition ParseCondition(string lua) {
        if (lua == null || lua.Length == 0) {
            return null;
        } else {
           return GetComponent<LuaContext>().CreateCondition(lua);
        }
    }

    public IEnumerator StepRoutine(EightDir dir) {
        if (tracking) {
            yield break;
        }

        location += dir.XY();

        if (GetComponent<CharaEvent>() == null) {
            yield return LinearStepRoutine(dir);
        } else {
            yield return GetComponent<CharaEvent>().StepRoutine(dir);
        }
    }

    public IEnumerator StepMultiRoutine(EightDir dir, int count) {
        for (int i = 0; i < count; i += 1) {
            yield return StartCoroutine(StepRoutine(dir));
        }
    }

    public IEnumerator LinearStepRoutine(EightDir dir) {
        tracking = true;
        targetPositionPx = TileToWorldCoords(location);
        while (true) {
            if (CalcTilesPerSecond() > 0) {
                positionPx = Vector3.MoveTowards(positionPx,
                    targetPositionPx,
                    CalcTilesPerSecond() * Time.deltaTime);
            } else {
                // indicates warp speed, cap'n
                positionPx = targetPositionPx;
            }

            if (positionPx == targetPositionPx) {
                tracking = false;
                break;
            } else {
                yield return null;
            }
        }
    }
}
