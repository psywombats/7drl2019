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

    public bool canActAgain { get; set; }
    public bool isRecovering { get; set; }
    public float maxCD { get; set; }

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

    public float GetMaxAscent() {
        return unit.stats.Get(StatTag.JUMP);
    }

    public float GetMaxDescent() {
        return unit.stats.Get(StatTag.JUMP) + 0.5f;
    }

    public bool CanUse(Skill skill) {
        return Get(StatTag.MP) >= skill.costMP && (skill.costCD == 0 || Get(StatTag.CD) == 0);
    }

    public int CalcDropDamage(float height) {
        if (Is(StatTag.O_FALL)) {
            return (int)(height * 2.0f);
        } else {
            float max = Mathf.Max(GetMaxDescent(), 3);
            return Mathf.RoundToInt(10.0f + (height - max) * 15.0f);
        }
    }

    // === ACTIONS =================================================================================

    public IEnumerator MeleeAttackRoutine(BattleUnit other) {
        yield return battler.AnimateAttackRoutine();

        int dmg = 0;
        if (RandUtils.Chance(Get(StatTag.ACC))) {
            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            dmg = (int)Get(StatTag.DMG);
            battle.Log(this + " attacked " + other + " for " + dmg + " damage.");
            if (Get(StatTag.ATTACKS) < 1) {
                isRecovering = true;
            }
        } else {
            battle.Log(this + " missed " + other + ".");
        }

        if (dmg > 0) {
            yield return other.TakeDamageRoutine(dmg, battler.damageAnimation);
        }
    }

    public IEnumerator TakeDamageRoutine(int damage, LuaAnimation damageAnimation) {
        if (!IsDead()) {
            unit.stats.Sub(StatTag.HP, damage);
            if (!tookDamageThisTurn) {
                yield return battler.AnimateTakeDamageRoutine();
            }
            tookDamageThisTurn = true;
            if (IsDead()) {
                yield return DieRoutine();
            }
        }
    }

    public IEnumerator DieRoutine() {
        // 7drl hack
        if (this != battle.pc && battle.pc.Get(StatTag.CD) > 0) {
            battle.pc.unit.stats.Sub(StatTag.CD, 1);
        }

        string flight = unit.flightMessages[Random.Range(0, unit.flightMessages.Count)];
        battle.Log(this + flight);
        battle.RemoveUnit(this);
        return battler.AnimateDieAction();
    }

    // === STATE MACHINE ===========================================================================

    public IEnumerator OnNewTurnRoutine() {
        tookDamageThisTurn = false;
        yield return null;
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
