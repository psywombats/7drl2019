using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PushEffect : Effector {

    public int knockbackMin;
    public int knockbackMax;
    public LuaAnimation damageAnimation;
    public int damageLow, damageHigh;

    public override IEnumerator ExecuteCellsRoutine(List<Vector2Int> locations) {
        yield return battler.SyncPlayAnim(skill.castAnim);

        foreach (Vector2Int location in locations) {
            BattleEvent target = map.GetEventAt<BattleEvent>(location);
            if (target == null) {
                continue;
            }
            BattleUnit other = target.unit;

            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            EightDir dir = battler.GetComponent<MapEvent>().DirectionTo(other.battler.GetComponent<MapEvent>());
            int power = Mathf.RoundToInt(Random.Range(knockbackMin, knockbackMax));

            other.battler.Knockback(dir, power);
            yield return CoUtils.Wait(0.2f);
            
            if (actor.Get(StatTag.ATTACKS) < 0.5) {
                actor.isRecovering = true;
            }

            if (damageHigh > 0) {
                int dmg = Mathf.RoundToInt(Random.Range(damageLow, damageHigh));
                battle.Log(other + " took " + dmg + " damage.");
                other.TakeDamage(dmg, damageAnimation);
            }
        }
    }
}
