using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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
    private const string PropertyVisible = "onVisible";

    public const string EventEnabled = "enabled";
    public const string EventCollide = "collide";
    public const string EventInteract = "interact";
    public const string EventMove = "move";

    // Editor properties
    [HideInInspector] public Vector2Int location;
    [HideInInspector] public Vector2Int size;
    [Space]
    [Header("Movement")]
    public float tilesPerSecond = 2.0f;
    public bool passable = true;
    public bool eventPassable = true;
    [Space]
    [Header("Lua scripting")]
    public string luaCondition;
    [TextArea(3, 6)] public string luaOnInteract;
    [TextArea(3, 6)] public string luaOnCollide;
    [TextArea(3, 6)] public string luaVisible;

    // Properties
    public LuaMapEvent luaObject { get; private set; }
    public Vector3 targetPositionPx { get; set; }
    public bool tracking { get; set; }

    private bool visTriggered;

    private Vector3 _internalPosition;
    public Vector3 positionPx {
        get { return _internalPosition; }
        set {
            _internalPosition = value;
            transform.localPosition = InternalPositionToDisplayPosition(_internalPosition);
        }
    }

    private Map _map;
    public Map map {
        get {
            // this is wiped in update but we'll cache it across frames anyway
            if (_map != null) {
                return _map;
            }
            GameObject parentObject = gameObject;
            while (parentObject.transform.parent != null) {
                parentObject = parentObject.transform.parent.gameObject;
                Map map = parentObject.GetComponent<Map>();
                if (map != null) {
                    _map = map;
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
        return EightDirExtensions.DirectionOf(location - this.location);
    }

    public EightDir DirectionTo(MapEvent other) {
        return EightDirExtensions.DirectionOf(other.location - location);
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
            luaObject.Set(PropertyVisible, luaVisible);

            positionPx = transform.localPosition;

            CheckEnabled();
        }
    }

    public virtual void Update() {
        if (Application.IsPlaying(this)) {
            CheckEnabled();
        }
        _map = null;
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
        return (passable && (eventPassable || GetComponent<PCEvent>() != null)) || !switchEnabled;
    }

    public OrthoDir OrthoDirTo(MapEvent other) {
        return OrthoDirExtensions.DirectionOf3D(location - other.location);
    }

    public bool CanPassAt(Vector2Int loc) {
        if (loc.x < 0 || loc.x >= map.width || loc.y < 0 || loc.y >= map.height) {
            return false;
        }
        if (!GetComponent<MapEvent>().switchEnabled || passable) {
            return true;
        }
        if (map.terrain.HeightAt(loc) == 0) {
            return false;
        }
        foreach (Tilemap layer in map.layers) {
            if (layer.transform.position.z >= map.objectLayer.transform.position.z && 
                    !map.IsChipPassableAt(layer, loc)) {
                return false;
            }
        }
        foreach (MapEvent mapEvent in map.GetEventsAt(loc)) {
            if (!mapEvent.IsPassableBy(this)) {
                return false;
            }
        }
        return true;
    }

    public IEnumerator PathToRoutine(Vector2Int location) {
        List<Vector2Int> path = map.FindPath(this, location);
        if (path == null) {
            yield break;
        }
        MapEvent mapEvent = GetComponent<MapEvent>();
        foreach (Vector2Int target in path) {
            yield return StartCoroutine(GetComponent<MapEvent>().StepRoutine(location, target));
        }
    }

    public void SetCameraTrackerLocation(Vector3 loc) {
        _internalPosition = InternalPositionToDisplayPosition(loc);
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

    public IEnumerator CheckIfVisibilityTriggeredRoutine() {
        if (!visTriggered && luaVisible.Length > 0) {
            if (Global.Instance().Maps.pc.GetComponent<BattleEvent>().CanSeeLocation(
                Global.Instance().Maps.pc.GetComponent<BattleEvent>().unit.battle.map.terrain,
                location)) {
                visTriggered = true;
                return OnVisibleRoutine(Global.Instance().Maps.pc);
            }
        }
        return null;
    }

    // we have a solid TileX/TileY, please move the doll to the correct screen space
    public abstract void SetScreenPositionToMatchTilePosition();

    // set the one xyz coordinate not controlled by arrow keys
    public abstract void SetDepth();

    protected abstract void DrawGizmoSelf();

    public bool IsAnimating() {
        if (GetComponent<CharaEvent>() == null) {
            return tracking;
        }
        return tracking || GetComponent<CharaEvent>().animationQueue.Count > 0;
    }

    // called when the pc stumbles into us
    // before the step if impassable, after if passable
    public IEnumerator CollideRoutine(PCEvent pc) {
        while (pc.GetComponent<MapEvent>().IsAnimating()) {
            yield return null;
        }
        yield return luaObject.RunRoutine(PropertyCollide);
    }

    // called when the avatar stumbles into us
    // facing us if impassable, on top of us if passable
    public IEnumerator InteractRoutine(PCEvent pc) {
        if (GetComponent<CharaEvent>() != null) {
            GetComponent<CharaEvent>().facing = DirectionTo(pc.GetComponent<MapEvent>());
        }
        yield return luaObject.RunRoutine(PropertyInteract);
    }

    public IEnumerator OnVisibleRoutine(PCEvent pc) {
        while (IsAnimating()) {
            yield return null;
        }
        BattleEvent ev = GetComponent<BattleEvent>();
        if (ev != null) {
            BattleUnit unit = ev.unit;
            var ui = FindObjectOfType<RogueUI>();
            yield return ui.PrepareTalkRoutine(unit);
            ui.face2.Populate(unit);

            pc.GetComponent<CharaEvent>().FaceToward(unit.battler.GetComponent<MapEvent>());
            unit.battler.GetComponent<CharaEvent>().FaceToward(pc.GetComponent<MapEvent>());

            LuaScript script = new LuaScript(ui.GetComponent<LuaContext>(), luaVisible);
            ui.GetComponent<LuaContext>().SetGlobal("name", unit.ToString());
            ui.rightDisplayEnabled = true;
            yield return script.RunRoutine();
            ui.rightDisplayEnabled = false;
        } else {
            yield return luaObject.RunRoutine(PropertyVisible);
        }
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
    public IEnumerator StepRoutine(EightDir dir, bool updateLoc = true) {
        return StepRoutine(location, location + dir.XY(), updateLoc);
    }
    public IEnumerator StepRoutine(Vector2Int at, Vector2Int to, bool updateLoc = true) {
        if (updateLoc) {
            location = to;
        }

        if (GetComponent<CharaEvent>() == null) {
            yield return LinearStepRoutine(at, to);
        } else {
            yield return GetComponent<CharaEvent>().StepRoutine(at, to);
        }
    }

    public IEnumerator StepMultiRoutine(EightDir dir, int count) {
        if (GetComponent<PCEvent>() != null) {
            map.GetComponent<LineOfSightEffect>().RecalculateVisibilityMap();
        }
        for (int i = 0; i < count; i += 1) {
            Vector2Int at = location;
            yield return StartCoroutine(StepRoutine(at, at + dir.XY()));
            at += dir.XY();
        }
    }

    public IEnumerator LinearStepRoutine(Vector2Int at, Vector2Int to) {
        Vector3 toPos = TileToWorldCoords(to);
        Vector3 fromPos = TileToWorldCoords(at);
        tracking = true;
        float elapsed = 0.0f;
        
        if (CalcTilesPerSecond() > 0) {
            float duration = 1.0f / CalcTilesPerSecond();
            while (true) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                positionPx = toPos * t + (1.0f - t) * fromPos;
                if (t > 1) {
                    break;
                }
                yield return null;
            }
        }
        positionPx = toPos;
        tracking = false;
    }
}
