using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicDamageEffect : Effector {

    public string effectName;
    public LuaAnimation castAnimation;
    public LuaAnimation damageAnimation;
    public int damageLow, damageHigh;

    public override IEnumerator ExecuteCellsRoutine(Result<IEnumerator> result, List<Vector2Int> locations) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        foreach (Vector2Int location in locations) {
            BattleEvent target = map.GetEventAt<BattleEvent>(location);
            if (target == null) {
                result.Cancel();
                yield break;
            }
            BattleUnit other = target.unit;
            
            toExecute.Add(battler.PlayAnimationAction(castAnimation));

            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            int dmg = Mathf.RoundToInt(Random.Range(damageLow, damageHigh));
            battle.Log(actor + " cast " + effectName + " on " + other + " for " + dmg + " damage.");

            toExecute.Add(other.TakeDamageAction(dmg, damageAnimation));
            result.value = CoUtils.RunSequence(toExecute.ToArray());
        }
    }
}
