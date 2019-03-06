using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicDamageEffect : Effector {
    
    public LuaAnimation damageAnimation;
    public int damageLow, damageHigh;
    public bool friendlyFire;

    public override IEnumerator ExecuteCellsRoutine(List<Vector2Int> locations) {
        battle.Log(actor + " cast " + skill.skillName + "!");
        yield return battler.PlayAnimationRoutine(skill.castAnim);

        foreach (Vector2Int location in locations) {
            BattleEvent target = map.GetEventAt<BattleEvent>(location);
            if (target == null || (!friendlyFire && target.unit.align == actor.unit.align)) {
                continue;
            }
            BattleUnit other = target.unit;
            
            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            int dmg = Mathf.RoundToInt(Random.Range(damageLow, damageHigh));
            battle.Log(other + " took " + dmg + " damage.");

            yield return other.TakeDamageRoutine(dmg, damageAnimation);
        }
    }
}
