using System.Collections;

public class MeleeTargeter : Targeter {

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result) {
        DirectionCursor cursor = battle.SpawnDirectionCursor(actor.location);

        Result<EightDir> dirResult = new Result<EightDir>();
        if (effect.TargetsHostiles()) {
            yield return cursor.SelectAdjacentUnitRoutine(dirResult, actor, DefaultUnitRule(effect));
        } else {
            yield return cursor.AwaitSelectionRoutine(battler, dirResult);
        }
        
        battle.DespawnDirCursor();

        if (dirResult.canceled) {
            result.Cancel();
        } else {
            yield return effect.ExecuteDirectionRoutine(result, dirResult.value);
        }
    }
}
