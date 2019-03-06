using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class BattleEvent : MonoBehaviour {

    public const float AttackHeightMax = 1.0f;

    [HideInInspector]
    public Unit unitSerialized;
    public BattleUnit unit { get; set; }
    public BattleController controller { get; private set; }
    public MapEvent me { get { return GetComponent<MapEvent>(); } }

    public LuaAnimation damageAnimation;
    public LuaAnimation deathAnimation;
    public LuaAnimation attackAnimation;
    public LuaAnimation bumpAnimation;

    private LuaContext involuntaryContext;

    public Vector2Int location { get { return GetComponent<MapEvent3D>().location; } }

    private TacticsTerrainMesh _terrain;
    public TacticsTerrainMesh terrain {
        get {
            if (_terrain == null) _terrain = me.map.terrain;
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
        me.SetLocation(unit.location);
    }

    public bool CanCrossTileGradient(Vector2Int from, Vector2Int to, bool cornerMode = false) {
        float sqDist = (from - to).sqrMagnitude;
        float fromHeight = terrain.HeightAt(from);
        float toHeight = me.map.terrain.HeightAt(to);
        if (toHeight == 0.0f) {
            return false;
        }
        if (sqDist > 2) {
            Debug.Assert(false);
            return false;
        }
        if (fromHeight < toHeight) {
            if (cornerMode) {
                return toHeight - fromHeight < 2;
            }
            if (toHeight - fromHeight > unit.GetMaxAscent()) {
                return false;
            }
        } else {
            if (fromHeight - toHeight > unit.GetMaxDescent()) {
                return false;
            }
        }
        if (sqDist > 1.0f) {
            return CanCrossTileGradient(from, new Vector2Int(from.x, to.y), true) &&
                CanCrossTileGradient(from, new Vector2Int(to.x, from.y), true);
        } else {
            return true;
        }
    }

    public bool CanSeeLocation(TacticsTerrainMesh mesh, Vector2Int location) {
        return MathHelper3D.InLos(mesh, this.location, location, unit.Get(StatTag.SIGHT));
    }

    public IEnumerator StepOrAttackRoutine(EightDir dir, Result<bool> executeResult) {
        while (me.tracking) {
            yield return null;
        }

        MapEvent parent = me;
        Vector2Int vectors = me.location;
        Vector2Int target = vectors + dir.XY();
        GetComponent<CharaEvent>().facing = dir;
        List<MapEvent> targetEvents = me.map.GetEventsAt(target);

        if (!GetComponent<BattleEvent>().CanCrossTileGradient(parent.location, target)) {
            executeResult.Cancel();
            yield break;
        }

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = parent.CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            passable &= targetEvent.IsPassableBy(parent);
        }
        
        if (passable) {
            me.location = target;
            StartCoroutine(me.StepRoutine(dir, false));
            if (GetComponent<PCEvent>() != null) {
                foreach (MapEvent targetEvent in toCollide) {
                    if (targetEvent.switchEnabled) {
                        yield return targetEvent.CollideRoutine(GetComponent<PCEvent>());
                    }
                }
            }
            executeResult.value = true;
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                if (GetComponent<PCEvent>() != null) {
                    if (targetEvent.switchEnabled && !targetEvent.IsPassableBy(parent)) {
                        yield return targetEvent.CollideRoutine(GetComponent<PCEvent>());
                    }
                }
                if (targetEvent.GetComponent<BattleEvent>() != null) {
                    BattleEvent other = targetEvent.GetComponent<BattleEvent>();
                    if (unit.align != other.unit.align) {
                        float h1 = unit.battle.map.terrain.HeightAt(location);
                        float h2 = unit.battle.map.terrain.HeightAt(target);
                        if (Mathf.Abs(h1 - h2) > AttackHeightMax) {
                            if (GetComponent<PCEvent>() != null) {
                                unit.battle.Log("Too high up to attack!");
                            }
                        } else {
                            yield return unit.MeleeAttackRoutine(other.unit);
                            executeResult.value = true;
                        }
                    }
                }
                // 7drl antipattern hack alert
                if (GetComponent<PCEvent>() != null && targetEvent.GetComponent<ChestEvent>() != null) {
                    ChestEvent chest = targetEvent.GetComponent<ChestEvent>();
                    if (!chest.opened) {
                        yield return chest.OpenRoutine(GetComponent<PCEvent>());
                    }
                    executeResult.value = true;
                }
            }
        }

        if (!executeResult.finished) {
            executeResult.Cancel();
        }
    }

    public IEnumerator KnockbackRoutine(EightDir dir, int power) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        float height = me.map.terrain.HeightAt(location);
        for (int i = 0; i < power; i += 1) {
            Vector2Int to = me.location + dir.XY();
            float toHeight = me.map.terrain.HeightAt(to);
            if (!me.CanPassAt(to) || toHeight > height) {
                break;
            }
            me.location = to;
            toExecute.Add(GetComponent<CharaEvent>().StepRoutine(dir, false));
            if (toHeight < height) {
                float delta = height - toHeight;
                if (delta > unit.GetMaxDescent()) {
                    int dmg = unit.CalcDropDamage(delta);
                    toExecute.Add(TakeFallDamageRoutine(dmg));
                    yield return StartCoroutine(CoUtils.RunSequence(toExecute.ToArray()));
                    yield break;
                }
            }
        }
        StartCoroutine(CoUtils.RunSequence(toExecute.ToArray()));
        yield return null;
    }

    public IEnumerator TakeFallDamageRoutine(int dmg) {
        yield return unit.TakeDamageRoutine(dmg, damageAnimation);
        controller.Log(unit + " took " + dmg + " damage in the fall!");
        yield return null;
    }

    public IEnumerator AnimateTakeDamageRoutine() {
        StartCoroutine(PlayAnimationRoutine(damageAnimation, involuntaryContext));
        yield return null;
    }

    public IEnumerator AnimateDieAction() {

        // we no longer consider ourselves to be a valid anything on the map
        enabled = false;

        yield return PlayAnimationRoutine(deathAnimation);
    }

    public IEnumerator AnimateAttackRoutine() {
        StartCoroutine(PlayAnimationRoutine(attackAnimation));
        yield return null;
    }

    public IEnumerator AnimateBumpRoutine() {
        yield return PlayAnimationRoutine(bumpAnimation);
    }

    public IEnumerator PlayAnimationRoutine(LuaAnimation anim, LuaContext context = null) {
        if (anim == null) {
            yield break;
        }
        GetComponent<CharaEvent>().doll.GetComponent<CharaAnimationTarget>().ConfigureToBattler(this);
        yield return GetComponent<CharaEvent>().doll.GetComponent<AnimationPlayer>().PlayAnimationRoutine(anim, context);
    }
}
