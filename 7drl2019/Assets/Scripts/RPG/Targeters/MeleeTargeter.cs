using System.Collections;
using UnityEngine;

public class MeleeTargeter : Targeter {

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        DirectionCursor cursor = battle.SpawnDirectionCursor(actor.location);

        Result<EightDir> dirResult = new Result<EightDir>();
        yield return cursor.SelectTargetDirRoutine(dirResult, actor, (Vector2Int v) => {
            float d = Mathf.Abs(actor.battler.transform.position.y - map.terrain.HeightAt(v.x, v.y));
            return d <= BattleEvent.AttackHeightMax && DefaultSelectRule(effect)(v);
        }, true);

        battle.DespawnDirCursor();
       
        if (dirResult.canceled) {
            result.Cancel();
        } else {
            float d = Mathf.Abs(actor.battler.transform.position.y -
                map.terrain.HeightAt(actor.battler.location + dirResult.value.XY()));
            if (d <= BattleEvent.AttackHeightMax) {
                yield return effect.ExecuteDirectionRoutine(dirResult.value);
                result.value = true;
            } else {
                result.Cancel();
            }
        }
    }

    public override IEnumerator TryAIUse(AIController ai, Effector effect) {
        if (Vector2Int.Distance(actor.location, ai.pc.location) < 1.5) {
            battle.Log(actor + " used " + skill.skillName);
            EightDir dir = EightDirExtensions.DirectionOf( ai.pc.location - actor.location);
            return effect.ExecuteDirectionRoutine(dir);
        } else {
            return null;
        }
    }
}
