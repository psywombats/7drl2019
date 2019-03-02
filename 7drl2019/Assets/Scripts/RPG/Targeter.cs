using System.Collections;
using UnityEngine;

/**
 * Abstract to cover single-empty-square-at-range vs one-direction. Serialized props on each
 * instance are stuff like range, radius... mostly simple.
 **/
public abstract class Targeter : ActorScriptableObject {

    /**
     * Acquire the targets, pass them to the effector via the appropriate method.
     */
    public IEnumerator ExecuteRoutine(Effector effect, Result<Effector> effectResult) {
        Result<bool> executedResult = new Result<bool>();
        yield return InternalExecuteRoutine(effect, executedResult);
        if (executedResult.canceled) {
            effectResult.Cancel();
        } else if (executedResult.value) {
            effectResult.value = effect;
        } else {
            yield return ExecuteRoutine(effect, effectResult);
        }
    }

    protected abstract IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result);
}
