using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;

public class BattleAnimationPlayer : MonoBehaviour {

    public DollTargetEvent attacker = null;
    public DollTargetEvent defender = null;
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

    public IEnumerator PlayAnimationRoutine(BattleAnimation anim) {
        this.anim = anim;
        yield return PlayAnimationRoutine();
    }

    public IEnumerator PlayAnimationRoutine() {
        SetUpLua();
        attacker.PrepareForAnimation(this);
        defender.PrepareForAnimation(this);
        isPlayingAnimation = true;
        yield return anim.ToScript().RunRoutine();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
        isPlayingAnimation = false;
    }
}
