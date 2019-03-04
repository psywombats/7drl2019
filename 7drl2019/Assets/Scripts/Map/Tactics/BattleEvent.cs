using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class BattleEvent : MonoBehaviour {

    [HideInInspector]
    public Unit unitSerialized;
    public BattleUnit unit { get; set; }
    public BattleController controller { get; private set; }

    public LuaAnimation damageAnimation;
    public LuaAnimation deathAnimation;
    public LuaAnimation attackAnimation;

    private LuaContext involuntaryContext;

    public Vector2Int location { get { return GetComponent<MapEvent3D>().location; } }

    private TacticsTerrainMesh _terrain;
    public TacticsTerrainMesh terrain {
        get {
            if (_terrain == null) _terrain = GetComponent<MapEvent>().map.terrain;
            return _terrain;
        }
    }

    public void Start() {
        involuntaryContext = gameObject.AddComponent<LuaContext>();
    }

    public void PopulateWithUnitData(Unit unitData) {
        Debug.Assert(!Application.isPlaying);
        unitSerialized = unitData;
        if (unitData != null) {
            GetComponent<CharaEvent>().spritesheet = unitData.appearance;
            gameObject.name = unitData.unitName;
        }
    }

    public void SetScreenPositionToMatchTilePosition() {
        GetComponent<MapEvent>().SetLocation(unit.location);
    }

    public bool CanCrossTileGradient(Vector2Int from, Vector2Int to) {
        float fromHeight = terrain.HeightAt(from);
        float toHeight = GetComponent<MapEvent>().map.terrain.HeightAt(to);
        if (toHeight == 0.0f) {
            return false;
        }
        if (fromHeight < toHeight) {
            if (toHeight - fromHeight > unit.GetMaxAscent()) {
                return false;
            }
        } else {
            if (fromHeight - toHeight > unit.GetMaxDescent()) {
                return false;
            }
        }
        if ((from - to).sqrMagnitude > 1.0f) {
            return CanCrossTileGradient(from, new Vector2Int(from.x, to.y)) &&
                CanCrossTileGradient(from, new Vector2Int(to.x, from.y));
        } else {
            return true;
        }
    }

    public bool CanSeeLocation(TacticsTerrainMesh mesh, Vector2Int location) {
        return MathHelper3D.InLos(mesh, this.location, location, unit.Get(StatTag.SIGHT));
    }

    public IEnumerator StepOrAttackAction(EightDir dir) {
        MapEvent parent = GetComponent<MapEvent>();
        Vector2Int vectors = GetComponent<MapEvent>().location;
        Vector2Int target = vectors + dir.XY();
        GetComponent<CharaEvent>().facing = dir;
        List<MapEvent> targetEvents = GetComponent<MapEvent>().map.GetEventsAt(target);

        if (!GetComponent<BattleEvent>().CanCrossTileGradient(parent.location, target)) {
            return null;
        }

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = parent.CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            passable &= targetEvent.IsPassableBy(parent);
        }
        
        List<IEnumerator> toExecute = new List<IEnumerator>();
        if (passable) {
            GetComponent<MapEvent>().location = target;
            toExecute.Add(GetComponent<MapEvent>().StepRoutine(dir, false));
            if (GetComponent<PCEvent>() != null) {
                foreach (MapEvent targetEvent in toCollide) {
                    if (targetEvent.switchEnabled) {
                        toExecute.Add(targetEvent.CollideRoutine(GetComponent<PCEvent>()));
                    }
                }
            }
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                if (GetComponent<PCEvent>() != null) {
                    if (targetEvent.switchEnabled && !targetEvent.IsPassableBy(parent)) {
                        toExecute.Add(targetEvent.CollideRoutine(GetComponent<PCEvent>()));
                    }
                }
                if (targetEvent.GetComponent<BattleEvent>() != null) {
                    BattleEvent other = targetEvent.GetComponent<BattleEvent>();
                    if (unit.align != other.unit.align) {
                        toExecute.Add(unit.MeleeAttackAction(other.unit));
                    }
                }
            }
        }
        return CoUtils.RunSequence(toExecute.ToArray());
    }

    public IEnumerator AnimateTakeDamageAction() {
        return PlayAnimationAction(damageAnimation, involuntaryContext);
    }

    public IEnumerator AnimateDieAction() {

        // we no longer consider ourselves to be a valid anything on the map
        enabled = false;

        return PlayAnimationAction(deathAnimation);
    }

    public IEnumerator AnimateAttackAction() {
        return PlayAnimationAction(attackAnimation);
    }

    private IEnumerator PlayAnimationAction(LuaAnimation anim, LuaContext context = null) {
        GetComponent<CharaEvent>().doll.GetComponent<CharaAnimationTarget>().ConfigureToBattler(this);
        return GetComponent<CharaEvent>().doll.GetComponent<AnimationPlayer>().PlayAnimationRoutine(anim, context);
    }
}
