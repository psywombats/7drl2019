using System.Collections;

public class MeleeTargeter : Targeter {

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        DirectionCursor cursor = battle.SpawnDirectionCursor(actor.location);

        Result<EightDir> dirResult = new Result<EightDir>();
        yield return cursor.SelectTargetDirRoutine(dirResult, actor, DefaultSelectRule(effect), true);

        battle.DespawnDirCursor();

        if (dirResult.canceled) {
            result.Cancel();
        } else {
            yield return effect.ExecuteDirectionRoutine(dirResult.value);
            result.value = true;
        }
    }
}
