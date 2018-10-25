using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;

[RequireComponent(typeof(DuelMap))]
public class BattleAnimationPlayer : MonoBehaviour {

    public DollTargetEvent attacker = null;
    public DollTargetEvent defender = null;
    public BattleAnimation anim = null;

    public bool isPlayingAnimation { get; private set; }

    public void Start() {
        Global.Instance().Lua.SetGlobal("attacker", attacker);
        Global.Instance().Lua.SetGlobal("defender", defender);
    }

    public void EditorReset() {
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
        isPlayingAnimation = false;
    }

    public IEnumerator PlayAnimationRoutine(BattleAnimation anim) {
        this.anim = anim;
        yield return PlayAnimationRoutine();
    }

    public IEnumerator PlayAnimationRoutine() {
        attacker.PrepareForAnimation();
        defender.PrepareForAnimation();
        isPlayingAnimation = true;
        yield return anim.ToScript().RunRoutine();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
        isPlayingAnimation = false;
    }
}
