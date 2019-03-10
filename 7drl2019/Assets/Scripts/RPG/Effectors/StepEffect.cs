using UnityEngine;
using System.Collections;

public class StepEffect : Effector {

    public override bool TargetsHostiles() {
        return false;
    }

    public override bool AcceptsFullGrids() {
        return false;
    }

    public override IEnumerator ExecuteDirectionRoutine(EightDir dir) {
        yield return battler.SyncPlayAnim(skill.castAnim);
        actorEvent.GetComponent<CharaEvent>().PerformWhenDoneAnimating(actorEvent.StepRoutine(dir));
    }
}
