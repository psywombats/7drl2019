using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Map))]
[RequireComponent(typeof(BattleAnimationPlayer))]
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
    
    public void ConfigureForDuel(BattleEvent attacker, BattleEvent defender) {
        Attacker().ConfigureToBattler(attacker);
        Defender().ConfigureToBattler(defender);
        Attacker().GetComponent<CharaEvent>().facing = OrthoDir.East;
        Defender().GetComponent<CharaEvent>().facing = OrthoDir.West;
        GetComponent<BattleAnimationPlayer>().attacker = Attacker().GetComponent<DollTargetEvent>();
        GetComponent<BattleAnimationPlayer>().defender = Defender().GetComponent<DollTargetEvent>();
    }

    // this needs to take an attack command
    public IEnumerator EnterMapRoutine(BattleEvent attacker, BattleEvent defender) {
        GetComponent<BattleAnimationPlayer>().anim = Resources.Load<BattleAnimation>("Animations/Battle/test_anim");
        ConfigureForDuel(attacker, defender);
        float duration = 0.6f;
        yield return TacticsCam.Instance().SwitchToDuelCamRoutine(
            attacker.GetComponent<MapEvent>(), 
            defender.GetComponent<MapEvent>());
        yield return new WaitForSeconds(0.6f);
        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.Delay(0.0f, TacticsCam.Instance().DuelZoomRoutine(8.0f, duration/2.0f)),
            CoUtils.Delay(duration/3.0f, Global.Instance().Maps.BlendController.BlendInDuelRoutine(duration*2.0f/3.0f)),
            CoUtils.Delay(duration/3.0f, DuelCam.Instance().TransitionInZoomRoutine(12.0f, duration/2.0f)),
        }, this);

        yield return new WaitForSeconds(0.5f);
        yield return GetComponent<BattleAnimationPlayer>().PlayAnimationRoutine();
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator ExitMapRoutine() {
        TacticsCam.Instance().ResetToTacticsMode();
        yield return Global.Instance().Maps.BlendController.FadeInTacticsRoutine(1.2f);
    }
}
