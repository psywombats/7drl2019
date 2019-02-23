using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Map))]
[RequireComponent(typeof(BattleAnimationPlayer))]
public class DuelMap : MonoBehaviour {

    private Dictionary<Doll.Type, Doll> targets;

    public void Start() {

        // TODO: presumably we load this from somewhere
        Global.Instance().Maps.activeDuelMap = this;

        targets = new Dictionary<Doll.Type, Doll>();
        foreach (DollTargetEvent target in GetComponent<Map>().GetEvents<DollTargetEvent>()) {
            targets[target.doll.type] = target.doll;
        }
    }

    public Doll GetTarget(Doll.Type type) {
        return targets[type];
    }

    public Doll Attacker() {
        return targets[Doll.Type.Attacker];
    }

    public Doll Defender() {
        return targets[Doll.Type.Defender];
    }
    
    public void ConfigureForDuel(BattleEvent attacker, BattleEvent defender) {
        Attacker().ConfigureToBattler(attacker);
        Defender().ConfigureToBattler(defender);
        Attacker().chara.facing = OrthoDir.East;
        Defender().chara.facing = OrthoDir.West;
        GetComponent<BattleAnimationPlayer>().attacker = Attacker();
        GetComponent<BattleAnimationPlayer>().defender = Defender();
    }

    // this needs to take an attack command
    public IEnumerator EnterMapRoutine(BattleEvent attacker, BattleEvent defender) {
        ConfigureForDuel(attacker, defender);
        float duration = 0.6f;
        yield return TacticsCam.Instance().SwitchToDuelCamRoutine(
            attacker.GetComponent<MapEvent3D>(), 
            defender.GetComponent<MapEvent3D>());
        yield return new WaitForSeconds(0.6f);
        yield return CoUtils.RunParallel(new IEnumerator[] {
            CoUtils.Delay(0.0f, TacticsCam.Instance().DuelZoomRoutine(8.0f, duration/2.0f)),
            CoUtils.Delay(duration/3.0f, Global.Instance().Maps.blendController.BlendInDuelRoutine(duration*2.0f/3.0f)),
            CoUtils.Delay(duration/3.0f, DuelCam.Instance().TransitionInZoomRoutine(12.0f, duration/2.0f)),
        }, this);

        yield return new WaitForSeconds(0.8f);
        yield return GetComponent<BattleAnimationPlayer>().PlayAnimationRoutine();
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator ExitMapRoutine() {
        TacticsCam.Instance().ResetToTacticsMode();
        yield return Global.Instance().Maps.blendController.FadeInTacticsRoutine(1.2f);
    }
}
