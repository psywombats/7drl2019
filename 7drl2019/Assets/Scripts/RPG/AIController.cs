using UnityEngine;
using System.Collections;

public class AIController : ScriptableObject {
    
    // called repeatedly by the battle while ai units still have moves left
    public IEnumerator PlayNextAIActionRoutine(BattleUnit unit) {
        unit.battle.TargetCameraToLocation(unit.location);
        yield return new WaitForSeconds(0.8f);

        // TODO: the ai
    }
}
