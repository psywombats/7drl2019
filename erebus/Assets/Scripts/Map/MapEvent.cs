using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * The generic "thing on the map" class for MGNE2. Usually comes from Tiled.
 */
[RequireComponent(typeof(Dispatch))]
[RequireComponent(typeof(LuaContext))]
[DisallowMultipleComponent]
public abstract class MapEvent : TiledInstantiated {
    
    public const string EventEnabled = "enabled";
    public const string EventCollide = "collide";
    public const string EventInteract = "interact";
    public const string EventMove = "move";

    public const string PropertyUnit = "unit";
    public const string PropertyTarget = "target";
    private const string PropertyCondition = "show";
    private const string PropertyInteract = "onInteract";
    private const string PropertyCollide = "onCollide";

    private static readonly string TypeChara = "Character";

    // Editor properties
    public float tilesPerSecond = 2.0f;
    public IntVector2 position;
    public bool passable = true;
    public string LuaCondition;
    [TextArea(3, 6)] public string luaOnInteract;
    [TextArea(3, 6)] public string luaOnCollide;

    // Properties
    public LuaMapEvent luaObject { get; private set; }
    public Vector3 targetPositionPx { get; set; }
    public bool tracking { get; private set; }

    public Vector3 positionPx {
        get { return transform.localPosition; }
        set { transform.localPosition = value; }
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

    private int _layerIndex = -1;
    public int layerIndex {
        get {
            // this is a perf optimization -- events can't change layer now
            if (_layerIndex != -1) {
                return _layerIndex;
            }
            for (int thisLayerIndex = 0; thisLayerIndex < parent.transform.childCount; thisLayerIndex += 1) {
                if (parent.transform.GetChild(thisLayerIndex).gameObject.GetComponent<ObjectLayer>() == layer) {
                    _layerIndex = thisLayerIndex;
                    return thisLayerIndex;
                }
            }
            Assert.IsTrue(false);
            return -1;
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

    // public abstract

    // if we moved in this direction, where in screenspace would we end up?
    public abstract Vector3 CalculateOffsetPositionPx(OrthoDir dir);

    // public

    public override void Populate(IDictionary<string, string> properties) {
        gameObject.AddComponent<Dispatch>();
        position = new IntVector2(0, 0);
        RectangleObject rect = GetComponent<RectangleObject>();
        SetInitialLocation(rect);

        // lua junk
        if (properties.ContainsKey(PropertyCondition)) {
            LuaCondition = properties[PropertyCondition];
        }
        if (properties.ContainsKey(PropertyCollide)) {
            luaOnCollide = properties[PropertyCollide];
        }
        if (properties.ContainsKey(PropertyInteract)) {
            luaOnInteract = properties[PropertyInteract];
        }

        // type assignment
        if (GetComponent<RuntimeTmxObject>().TmxType == TypeChara && GetComponent<CharaEvent>() == null) {
            gameObject.AddComponent<CharaEvent>().Populate(properties);
        }
        if (properties.ContainsKey(PropertyUnit) && GetComponent<BattleEvent>() == null) {
            gameObject.AddComponent<BattleEvent>().Populate(properties);
        }
        if (properties.ContainsKey(PropertyTarget) && GetComponent<DollTargetEvent>() == null) {
            gameObject.AddComponent<DollTargetEvent>().Populate(properties);
        }

        SetDepth();
    }

    public void Awake() {
        luaObject = new LuaMapEvent(this);
        luaObject.Set(PropertyCollide, luaOnCollide);
        luaObject.Set(PropertyInteract, luaOnInteract);
        luaObject.Set(PropertyCondition, LuaCondition);
    }

    public void Start() {
        GetComponent<Dispatch>().RegisterListener(EventCollide, (object payload) => {
            OnCollide((AvatarEvent)payload);
        });
        GetComponent<Dispatch>().RegisterListener(EventInteract, (object payload) => {
            OnInteract((AvatarEvent)payload);
        });

        CheckEnabled();
    }

    public void Update() {
        SetDepth();
        CheckEnabled();

        // TODO: only clear this when we change scene for the avatar
        _parent = null;
    }

    public void OnValidate() {
        SetScreenPositionToMatchTilePosition();
        SetDepth();
    }

    public void CheckEnabled() {
        switchEnabled = luaObject.EvaluateBool(PropertyCondition, true);
    }

    public OrthoDir DirectionTo(MapEvent other) {
        return OrthoDirExtensions.DirectionOf(other.position - position);
    }

    public bool IsPassableBy(CharaEvent chara) {
        return passable || !switchEnabled;
    }

    public bool CanPassAt(IntVector2 loc) {
        if (loc.x < 0 || loc.x >= parent.width || loc.y < 0 || loc.y >= parent.height) {
            return false;
        }
        int thisLayerIndex = GetComponent<MapEvent>().layerIndex;
        for (int i = thisLayerIndex - 1; i >= 0 && i >= thisLayerIndex - 2; i -= 1) {
            TileLayer layer = parent.transform.GetChild(i).GetComponent<TileLayer>();
            if (!parent.IsChipPassableAt(layer, loc)) {
                return false;
            }
        }

        return true;
    }

    public bool ContainsPosition(IntVector2 loc) {
        if (GetComponent<RectangleObject>() == null) {
            return loc == position;
        }
        IntVector2 pos1 = position;
        IntVector2 pos2 = position;
        pos2.x += (int)((GetComponent<RectangleObject>().TmxSize.x / Map.TileSizePx) - 1);
        pos2.y += (int)((GetComponent<RectangleObject>().TmxSize.y / Map.TileSizePx) - 1);
        return loc.x >= pos1.x && loc.x <= pos2.x && loc.y >= pos1.y && loc.y <= pos2.y;
    }

    public void SetLocation(IntVector2 location) {
        position = location;
        OnValidate();
    }

    // we have a solid TileX/TileY, please move the doll to the correct screen space
    public abstract void SetScreenPositionToMatchTilePosition();

    // set the one xyz coordinate not controlled by arrow keys
    protected abstract void SetDepth();

    // set the initial place we start in from Tiled
    protected abstract void SetInitialLocation(RectangleObject rect);

    // called when the avatar stumbles into us
    // before the step if impassable, after if passable
    private void OnCollide(AvatarEvent avatar) {
        luaObject.Run(PropertyCollide);
    }

    // called when the avatar stumbles into us
    // facing us if impassable, on top of us if passable
    private void OnInteract(AvatarEvent avatar) {
        if (GetComponent<CharaEvent>() != null) {
            GetComponent<CharaEvent>().facing = DirectionTo(avatar.GetComponent<MapEvent>());
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

    public IEnumerator StepRoutine(OrthoDir dir) {
        if (tracking) {
            yield break;
        }
        tracking = true;
        
        position += dir.XY();
        targetPositionPx = CalculateOffsetPositionPx(dir);
        GetComponent<Dispatch>().Signal(EventMove, dir);

        while (true) {
            if (tilesPerSecond > 0) {
                positionPx = Vector3.MoveTowards(positionPx, 
                    targetPositionPx, 
                    tilesPerSecond * Time.deltaTime);
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

    public IEnumerator StepMultiRoutine(OrthoDir dir, int count) {
        for (int i = 0; i < count; i += 1) {
            yield return StartCoroutine(StepRoutine(dir));
        }
    }
}
