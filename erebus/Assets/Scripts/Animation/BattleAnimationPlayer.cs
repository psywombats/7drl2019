using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LuaContext))]
public class BattleAnimationPlayer : AnimationPlayer {

    public Doll attacker = null;
    public Doll defender = null;

    public override void EditorReset() {
        base.EditorReset();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
    }

    private void SetUpLua() {
        GetComponent<LuaContext>().SetGlobal("attacker", attacker);
        GetComponent<LuaContext>().SetGlobal("defender", defender);
        GetComponent<LuaContext>().SetGlobal("battle", defender);
    }

    public IEnumerator PlayAnimationRoutine(LuaAnimation anim, Doll attacker, Doll defender) {
        this.attacker = attacker;
        this.defender = defender;
        yield return PlayAnimationRoutine(anim);
    }

    public override IEnumerator PlayAnimationRoutine() {
        if (attacker == null || defender == null) {
            foreach (Doll doll in FindObjectsOfType<Doll>()) {
                if (doll.type == Doll.Type.Attacker) {
                    attacker = doll;
                } else if (doll.type == Doll.Type.Defender) {
                    defender = doll;
                }
            }
        }

        SetUpLua();
        attacker.PrepareForBattleAnimation(this, Doll.Type.Attacker);
        defender.PrepareForBattleAnimation(this, Doll.Type.Defender);
        yield return base.PlayAnimationRoutine();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
    }
}
