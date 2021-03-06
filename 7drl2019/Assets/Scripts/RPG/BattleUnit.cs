﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// representation of a unit in battle
public class BattleUnit {

    public Unit unit { get; private set; }
    public BattleController battle { get; set; }
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

    private AIController _ai;
    public AIController ai {
        get {
            if (_ai == null) _ai = new AIController(this);
            return _ai;
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
        float baseJump = unit.stats.Get(StatTag.JUMP);
        if (baseJump > 0) {
            return baseJump + 0.5f;
        } else {
            return 0;
        }
    }

    public bool CanUse(Skill skill) {
        return Get(StatTag.MP) >= skill.costMP && (skill.costCD == 0 || Get(StatTag.CD) == 0);
    }

    public int CalcDropDamage(float height) {
        if (Is(StatTag.O_FALL)) {
            return (int)(height * 2.0f);
        } else {
            float max = Mathf.Max(GetMaxDescent(), 3);
            return Mathf.RoundToInt(10.0f + (height - max) * 25.0f);
        }
    }

    // === ACTIONS =================================================================================

    public void MeleeAttack(BattleUnit other) {
        battler.AnimateAttack();

        int dmg = 0;
        if (RandUtils.Chance(Get(StatTag.ACC) / 100.0f)) {
            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            dmg = (int)Get(StatTag.DMG);
            battle.Log(this + " attacked " + other + " for " + dmg + " damage.");
            if (Get(StatTag.ATTACKS) < 1) {
                isRecovering = true;
            } else if (Get(StatTag.ATTACKS) > 1) {
                canActAgain = true;
            }
        } else {
            battle.Log(this + " missed " + other + ".");
        }

        if (dmg > 0) {
            other.TakeDamage(dmg, battler.damageAnimation);
        }
    }

    public void TakeDamage(int damage, LuaAnimation damageAnimation) {
        if (!IsDead()) {
            unit.stats.Sub(StatTag.HP, damage);
            if (!tookDamageThisTurn) {
                battler.PlayAnimation(damageAnimation);
            }
            tookDamageThisTurn = true;
            if (IsDead()) {
                Die();
            }
        }
    }

    public void Die() {
        // 7drl hack
        if (this != battle.pc && battle.pc.Get(StatTag.CD) > 0) {
            battle.pc.unit.stats.Sub(StatTag.CD, 1);
        }

        string flight = unit.flightMessages[Random.Range(0, unit.flightMessages.Count)];
        battle.Log(this + flight);
        battle.RemoveUnit(this);
        battler.AnimateDie();
        battler.GetComponent<MapEvent>().enabled = false;
        battler.enabled = false;
    }

    public void OnNewTurn() {
        tookDamageThisTurn = false;
    }

    // === UTIL ====================================================================================

    public override string ToString() {
        if (!unit.unique) {
            if (unit.unitName.StartsWith("a") || unit.unitName.StartsWith("e") || unit.unitName.StartsWith("i") 
                || unit.unitName.StartsWith("o") || unit.unitName.StartsWith("u")) {
                return "an " + unit.unitName;
            } else {
                return "a " + unit.unitName;
            }
        } else {
            return unit.unitName;
        }
    }

    public string StatusString() {
        string result = this + "." +
            " Hits for " + Get(StatTag.DMG) + " at " + Get(StatTag.ACC) + " accuracy";
        if (Get(StatTag.SIGHT) < 8) {
            result += ", is shortsighted";
        }
        if (Get(StatTag.MOVE) > 1) {
            result += ", covers ground quickly";
        }
        if (Get(StatTag.MOVE) < 1) {
            result += ", covers ground slowly";
        }
        if (Get(StatTag.JUMP) > 1.5) {
            result += ", can jump very high";
        }
        if (Get(StatTag.JUMP) < 0.5) {
            result += ", can't jump";
        }
        if (Get(StatTag.ATTACKS) > 1) {
            result += ", attacks quickly";
        }
        if (Get(StatTag.ATTACKS) < 1) {
            result += ", attacks slowly";
        }
        result += ".";
        return result;
    }
}
