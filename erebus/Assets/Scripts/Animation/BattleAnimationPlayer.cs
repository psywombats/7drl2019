using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LuaContext))]
public class BattleAnimationPlayer : AnimationPlayer {

    public CharaAnimationTarget attacker = null;
    public CharaAnimationTarget defender = null;
    public Item debugItem;

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

    public IEnumerator PlayAnimationRoutine(LuaAnimation anim, CharaAnimationTarget attacker, CharaAnimationTarget defender) {
        this.attacker = attacker;
        this.defender = defender;
        yield return PlayAnimationRoutine(anim);
    }

    public override IEnumerator PlayAnimationRoutine() {
        if (attacker == null || defender == null) {
            foreach (CharaAnimationTarget doll in FindObjectsOfType<CharaAnimationTarget>()) {
                if (doll.type == CharaAnimationTarget.Type.Attacker) {
                    attacker = doll;
                } else if (doll.type == CharaAnimationTarget.Type.Defender) {
                    defender = doll;
                }
            }
        }

        if (debugItem != null) {
            attacker.chara.itemSprite = debugItem.sprite;
            defender.chara.itemSprite = debugItem.sprite;
        }

        SetUpLua();
        attacker.PrepareForBattleAnimation(this, CharaAnimationTarget.Type.Attacker);
        defender.PrepareForBattleAnimation(this, CharaAnimationTarget.Type.Defender);
        yield return base.PlayAnimationRoutine();
        attacker.ResetAfterAnimation();
        defender.ResetAfterAnimation();
    }
}
