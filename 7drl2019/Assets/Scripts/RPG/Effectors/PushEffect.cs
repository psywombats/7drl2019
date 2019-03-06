using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PushEffect : Effector {

    public int knockbackMin;
    public int knockbackMax;

    public override IEnumerator ExecuteCellsRoutine(List<Vector2Int> locations) {
        List<IEnumerator> toExecute = new List<IEnumerator>();
        battle.Log(actor + " cast " + skill.skillName + "!");
        yield return battler.PlayAnimationRoutine(skill.castAnim);

        foreach (Vector2Int location in locations) {
            BattleEvent target = map.GetEventAt<BattleEvent>(location);
            if (target == null) {
                continue;
            }
            BattleUnit other = target.unit;

            battler.GetComponent<CharaEvent>().FaceToward(other.battler.GetComponent<MapEvent>());
            EightDir dir = battler.GetComponent<MapEvent>().DirectionTo(other.battler.GetComponent<MapEvent>());
            int power = Mathf.RoundToInt(Random.Range(knockbackMin, knockbackMax));

            yield return other.battler.KnockbackRoutine(dir, power);
        }
    }
}
