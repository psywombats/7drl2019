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
    public BattleController battle { get { return unit.battle; } }
    public MapEvent me { get { return GetComponent<MapEvent>(); } }
    public CharaEvent chara { get { return GetComponent<CharaEvent>(); } }

    public LuaAnimation damageAnimation;
    public LuaAnimation deathAnimation;
    public LuaAnimation attackAnimation;
    public LuaAnimation bumpAnimation;

    private LuaContext involuntaryContext;
    private float sight;

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
                return toHeight - fromHeight <= 0.5f;
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
    
    public bool CanSeeLocation(TacticsTerrainMesh mesh, Vector2Int to) {
        LineOfSightEffect los = battle.GetComponent<LineOfSightEffect>();
        if (los.sitemap == null) {
            los.RegenSitemap(mesh);
        }
        if (sight == 0) {
            sight = unit.Get(StatTag.SIGHT);
        }
        //return MathHelper3D.InLos(mesh, this.location, location, sight);
        Vector2Int at = location;
        Vector3 at3 = new Vector3(at.x + 0.5f, mesh.HeightAt(at.x, at.y) + 1.5f, at.y + 0.5f);
        Vector3 to3 = new Vector3(to.x + 0.5f, mesh.HeightAt(to.x, to.y) + 1.5f, to.y + 0.5f);
        Vector3 delta = (to3 - at3);
        if (delta.sqrMagnitude > sight * sight) {
            return false;
        } else {
            return los.sitemap[
                            to.y * (mesh.size.x * mesh.size.y * mesh.size.x) +
                            to.x * (mesh.size.x * mesh.size.y) +
                            at.y * (mesh.size.x) +
                            at.x];
        }
    }

    public void StepOrAttack(EightDir dir, Result<bool> executeResult) {
        MapEvent parent = me;
        Vector2Int vectors = me.location;
        Vector2Int target = vectors + dir.XY();
        GetComponent<CharaEvent>().facing = dir;
        List<MapEvent> targetEvents = me.map.GetEventsAt(target);

        if (!GetComponent<BattleEvent>().CanCrossTileGradient(parent.location, target)) {
            executeResult.value = false;
            return;
        }

        List<MapEvent> toCollide = new List<MapEvent>();
        bool passable = parent.CanPassAt(target);
        foreach (MapEvent targetEvent in targetEvents) {
            toCollide.Add(targetEvent);
            passable &= targetEvent.IsPassableBy(parent);
        }
        
        if (passable) {
            chara.PerformWhenDoneAnimating(me.StepRoutine(location, location + dir.XY(), false));
            me.location = target;
            if (unit.Get(StatTag.MOVE) > 1) {
                unit.canActAgain = !unit.canActAgain;
            } else if (unit.Get(StatTag.MOVE) < 1) {
                unit.isRecovering = true;
            }
            if (GetComponent<PCEvent>() != null) {
                foreach (MapEvent targetEvent in toCollide) {
                    if (targetEvent.switchEnabled) {
                        StartCoroutine(CoUtils.RunWithCallback(targetEvent.CollideRoutine(GetComponent<PCEvent>()),
                            () => {
                                executeResult.value = true;
                            }));
                        return;
                    }
                }
            }
            executeResult.value = true;
            return;
        } else {
            foreach (MapEvent targetEvent in toCollide) {
                float h1 = unit.battle.map.terrain.HeightAt(location);
                float h2 = unit.battle.map.terrain.HeightAt(target);
                //if (GetComponent<PCEvent>() != null) {
                //    if (targetEvent.switchEnabled && !targetEvent.IsPassableBy(parent) 
                //            && Mathf.Abs(h1 - h2) <= AttackHeightMax) {
                //        StartCoroutine(CollideRoutine(GetComponent<PCEvent>());
                //    }
                //}
                if (targetEvent.GetComponent<BattleEvent>() != null) {
                    BattleEvent other = targetEvent.GetComponent<BattleEvent>();
                    if (unit.align != other.unit.align) {
                        if (Mathf.Abs(h1 - h2) > AttackHeightMax) {
                            if (GetComponent<PCEvent>() != null) {
                                unit.battle.Log("Too high up to attack!");
                            }
                            executeResult.value = false;
                            return;
                        } else {
                            unit.MeleeAttack(other.unit);
                            executeResult.value = true;
                            return;
                        }
                    }
                }
                // 7drl antipattern hack alert
                if (GetComponent<PCEvent>() != null && targetEvent.GetComponent<ChestEvent>() != null) {
                    ChestEvent chest = targetEvent.GetComponent<ChestEvent>();
                    if (!chest.opened) {
                        StartCoroutine(CoUtils.RunWithCallback(chest.OpenRoutine(GetComponent<PCEvent>()), () => {
                            executeResult.value = true;
                        }));
                        return;
                    }
                }
            }
        }
        executeResult.value = false;
    }

    public void Knockback(EightDir dir, int power) {
        float height = me.map.terrain.HeightAt(location);
        for (int i = 0; i < power; i += 1) {
            Vector2Int to = me.location + dir.XY();
            float toHeight = me.map.terrain.HeightAt(to);
            if (!me.CanPassAt(to) || toHeight > height) {
                break;
            }
            chara.PerformWhenDoneAnimating(GetComponent<CharaEvent>().StepRoutine(location, to, false));
            me.location = to;
            if (toHeight < height) {
                float delta = height - toHeight;
                if (delta > unit.GetMaxDescent()) {
                    int dmg = unit.CalcDropDamage(delta);
                    battle.Log(unit + " took " + dmg + " damage in the fall!");
                    unit.TakeDamage(dmg, damageAnimation);
                    break;
                }
            }
        }
    }

    public void AnimateTakeDamage() {
        PlayAnimation(damageAnimation, null, involuntaryContext);
    }

    public void AnimateDie() {
        if (!GetComponent<PCEvent>()) {
            // we no longer consider ourselves to be a valid anything on the map
            enabled = false;
            PlayAnimation(deathAnimation, GetComponent<MapEvent>());
        }
    }

    public void AnimateAttack() {
        PlayAnimation(attackAnimation);
    }

    public void AnimateBump() {
        PlayAnimation(bumpAnimation);
    }

    public void PlayAnimation(LuaAnimation anim, MapEvent kill = null, LuaContext context = null) {
        if (anim == null) {
            return;
        }
        chara.doll.GetComponent<CharaAnimationTarget>().ConfigureToBattler(this);
        chara.PerformWhenDoneAnimating(
            CoUtils.RunWithCallback(chara.doll.GetComponent<AnimationPlayer>().PlayAnimationRoutine(anim, context), () => {
                if (kill != null) {
                    battle.map.RemoveEvent(kill);
                }
            }));
    }

    public IEnumerator SyncPlayAnim(LuaAnimation anim, LuaContext context = null) {
        PlayAnimation(anim, null, context);
        yield return FinishAnims();
    }

    public IEnumerator FinishAnims() {
        while (GetComponent<MapEvent>().IsAnimating()) {
            yield return null;
        }
    }
}
