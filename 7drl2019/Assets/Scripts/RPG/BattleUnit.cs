using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// representation of a unit in battle
public class BattleUnit {

    public Unit unit { get; private set; }
    public BattleController battle { get; set; }
    public AIController ai { get; set; }
    public Alignment align { get { return unit.align; } }
    public Vector2Int location { get { return battler.location; } }

    private bool tookDamageThisTurn;

    public BattleEvent battler {
        get {
            return battle.GetEventForBattler(this);
        }
    }

    // === INITIALIZATION ==========================================================================

    //  - unit, this is a keyed by what comes in from tiled and used to look up hero/enemy in db
    //  - battle, the parent battle creating this unit for
    public BattleUnit(Unit unit, BattleController battle) {
        this.unit = unit;
        this.battle = battle;
    }

    // === RPG =====================================================================================

    public float Get(StatTag tag) {
        return unit.stats.Get(tag);
    }

    public bool Is(StatTag tag) {
        return unit.stats.Is(tag);
    }

    // checks for deadness and dead-like conditions like petrification
    public bool IsDead() {
        return unit.stats.Get(StatTag.HP) <= 0;
    }

    public int GetMaxAscent() {
        return (int)unit.stats.Get(StatTag.JUMP);
    }

    public int GetMaxDescent() {
        return (int)unit.stats.Get(StatTag.JUMP) + 1;
    }

    public bool CanUse(Skill skill) {
        return Get(StatTag.MP) >= skill.mpCost;
    }

    // === ACTIONS =================================================================================

    public IEnumerator MeleeAttackAction(BattleUnit other) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        toExecute.Add(battler.AnimateAttackAction());

        battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
        int dmg = Mathf.RoundToInt(Random.Range(Get(StatTag.DMG_MIN), Get(StatTag.DMG_MAX)));
        battle.Log(this + " attacked " + other + " for " + dmg + " damage.");

        if (dmg > 0) {
            toExecute.Add(other.TakeDamageAction(dmg, battler.damageAnimation));
        }
        return CoUtils.RunSequence(toExecute.ToArray());
    }

    public IEnumerator TakeDamageAction(int damage, LuaAnimation damageAnimation) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        if (!IsDead()) {
            unit.stats.Sub(StatTag.HP, damage);
            if (!tookDamageThisTurn) {
                toExecute.Add(battler.PlayAnimationAction(damageAnimation));
            }
            tookDamageThisTurn = true;
            if (IsDead()) {
                toExecute.Add(DieAction());
            }
        }
        return CoUtils.RunSequence(toExecute.ToArray());
    }

    public IEnumerator DieAction() {
        battle.Log(this + " is defeated.");
        battle.RemoveUnit(this);
        return battler.AnimateDieAction();
    }

    // === STATE MACHINE ===========================================================================

    public IEnumerator OnNewTurnAction() {
        tookDamageThisTurn = false;
        return null;
    }

    // === UTIL ====================================================================================

    public override string ToString() {
        if (!unit.unique) {
            return "the " + unit.unitName;
        } else {
            return unit.unitName;
        }
    }
}
