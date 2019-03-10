using UnityEngine;
using System.Collections;

public class NoneTargeter : Targeter {

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        result.value = true;
        yield return null;
    }

    public override IEnumerator TryAIUse(AIController ai, Effector effect) {
        return null;
    }
}
