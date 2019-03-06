﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicDamageEffect : Effector {
    
    public LuaAnimation damageAnimation;
    public int damageLow, damageHigh;
    public bool friendlyFire;

    public override IEnumerator ExecuteCellsRoutine(Result<IEnumerator> result, List<Vector2Int> locations) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        battle.Log(actor + " cast " + skill.skillName + "!");

        foreach (Vector2Int location in locations) {
            BattleEvent target = map.GetEventAt<BattleEvent>(location);
            if (target == null || (!friendlyFire && target.unit.align == actor.unit.align)) {
                continue;
            }
            BattleUnit other = target.unit;
            
            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            int dmg = Mathf.RoundToInt(Random.Range(damageLow, damageHigh));
            battle.Log(other + " took " + dmg + " damage.");

            toExecute.Add(other.TakeDamageAction(dmg, damageAnimation));
        }
        result.value = CoUtils.RunSequence(toExecute.ToArray());
        yield return CoUtils.RunParallel(toExecute.ToArray(), battler);
    }
}
