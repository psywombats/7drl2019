using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Map))]
public class DuelMap : MonoBehaviour {

    private Dictionary<DollTargetEvent.Type, DollTargetEvent> targets;

    public void Start() {

        // TODO: presumably we load this from somewhere
        Global.Instance().Maps.ActiveDuelMap = this;

        targets = new Dictionary<DollTargetEvent.Type, DollTargetEvent>();
        foreach (DollTargetEvent target in GetComponent<Map>().GetEvents<DollTargetEvent>()) {
            targets[target.type] = target;
        }
    }

    public DollTargetEvent GetTarget(DollTargetEvent.Type type) {
        return targets[type];
    }

    public DollTargetEvent Attacker() {
        return targets[DollTargetEvent.Type.Attacker];
    }

    public DollTargetEvent Defender() {
        return targets[DollTargetEvent.Type.Defender];
    }

    // these methods should eventually all take an event or something
    public void ConfigureForDuel(BattleEvent attacker, BattleEvent defender) {
        Attacker().ConfigureToBattler(attacker);
        Defender().ConfigureToBattler(defender);
        Attacker().GetComponent<CharaEvent>().facing = OrthoDir.East;
        Defender().GetComponent<CharaEvent>().facing = OrthoDir.West;
    }

    public IEnumerator SwitchToMapRoutine(BattleEvent attacker, BattleEvent defender) {
        ConfigureForDuel(attacker, defender);
        yield return TacticsCam.Instance().SwitchToDuelCamRoutine(
            attacker.GetComponent<MapEvent>(), 
            defender.GetComponent<MapEvent>());
        yield return new WaitForSeconds(1.0f);
        yield return Global.Instance().Maps.BlendController.BlendInDuelRoutine(1.0f);
    }
}
