using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;

public class BattleAnimationPlayer : MonoBehaviour {

    public Doll attacker = null;
    public Doll defender = null;
    public BattleAnimation anim = null;

    public bool isPlayingAnimation { get; private set; }

    public void EditorReset() {
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
        isPlayingAnimation = false;
    }

    private void SetUpLua() {
        Global.Instance().Lua.SetGlobal("attacker", attacker);
        Global.Instance().Lua.SetGlobal("battle", attacker); // lol, too cheap to set as global
        Global.Instance().Lua.SetGlobal("defender", defender);
    }

    public IEnumerator PlayAnimationRoutine(BattleAnimation anim, Doll attacker, Doll defender) {
        this.attacker = attacker;
        this.defender = defender;
        SetUpLua();
        yield return PlayAnimationRoutine(anim);
    }

    public IEnumerator PlayAnimationRoutine(BattleAnimation anim) {
        this.anim = anim;
        yield return PlayAnimationRoutine();
    }

    public IEnumerator PlayAnimationRoutine() {
        SetUpLua();
        attacker.PrepareForBattleAnimation(this, Doll.Type.Attacker);
        defender.PrepareForBattleAnimation(this, Doll.Type.Defender);
        isPlayingAnimation = true;
        yield return anim.ToScript().RunRoutine();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
        isPlayingAnimation = false;
    }
}
