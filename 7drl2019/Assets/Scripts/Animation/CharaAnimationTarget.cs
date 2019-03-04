using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using DG.Tweening;

[MoonSharpUserData]
[DisallowMultipleComponent]
public class CharaAnimationTarget : AnimationTarget {

    private static string ArgLayer = "layer";
    private static string ArgMode = "mode";

    private static float DefaultJumpHeight = 1.2f;
    private static float DefaultJumpReturnHeight = 0.4f;

    public enum Type {
        Attacker,
        Defender,
    }

    public Type type;
    public BattleAnimationPlayer player;
    public CharaEvent chara { get { return transform.parent.GetComponent<CharaEvent>(); } }

    private Vector3 originalDollPos;

    [MoonSharpHidden]
    public void ConfigureToBattler(BattleEvent battler) {
        chara.itemSprite = battler.unit.unit.equippedItem.sprite;
    }

    [MoonSharpHidden]
    public void PrepareForBattleAnimation(BattleAnimationPlayer player, Type type) {
        this.player = player;
        this.type = type;
        originalDollPos = transform.position;
    }

    [MoonSharpHidden]
    public override void ResetAfterAnimation() {
        transform.position = originalDollPos;
        foreach (SpriteRenderer renderer in renderers) {
            renderer.GetComponent<AfterimageComponent>().enabled = false;
        }
        chara.overrideBodySprite = null;
        chara.itemSprite = null;
        chara.armMode = ArmMode.Disabled;
        chara.itemMode = ItemMode.Disabled;
    }

    // === COMMAND HELPERS =========================================================================


    private Vector3 CalculateJumpOffset(Vector3 startPos, Vector3 endPos) {
        Vector3 dir = (endPos - startPos).normalized;
        return endPos - 1.15f * dir;
    }

    private IEnumerator JumpRoutine(Vector3 endPos, float duration, float height) {
        Vector3 startPos = transform.position;
        float elapsed = 0.0f;
        while (transform.position != endPos) {
            elapsed += Time.deltaTime;
            Vector3 lerped = Vector3.Lerp(startPos, endPos, elapsed / duration);
            transform.position = new Vector3(
                    lerped.x,
                    lerped.y + ((elapsed >= duration)
                            ? 0
                            : Mathf.Sin(elapsed / duration * Mathf.PI) * height),
                    lerped.z);
            yield return null;
        }
    }

    // === LUA FUNCTIONS ===========================================================================

    // jumpToDefender({});
    public void jumpToDefender(DynValue args) { CSRun(cs_jumpToDefender(args), args); }
    private IEnumerator cs_jumpToDefender(DynValue args) {
        chara.jumping = true;
        Vector3 endPos = CalculateJumpOffset(transform.position, player.defender.transform.position);
        float duration = (float)args.Table.Get(ArgDuration).Number;
        yield return JumpRoutine(endPos, duration, DefaultJumpHeight);
        chara.jumping = false;
    }

    // jumpReturn({duration?});
    public void jumpReturn(DynValue args) { CSRun(cs_jumpReturn(args), args); }
    private IEnumerator cs_jumpReturn(DynValue args) {
        float overallDuration = (float)args.Table.Get(ArgDuration).Number;
        float fraction = (2.0f / 3.0f);
        Vector3 midPos = Vector3.Lerp(transform.position, originalDollPos, fraction);
        yield return JumpRoutine(midPos,
                    overallDuration * fraction,
                    DefaultJumpReturnHeight * fraction);
        yield return JumpRoutine(originalDollPos,
                overallDuration * (1.0f - fraction) * 1.5f,
                DefaultJumpReturnHeight * (1.0f - fraction));
    }

    // setFrame({frame, layer=0});
    public void setBody(DynValue args) {
        int spriteFrame = (int)args.Table.Get(ArgFrame).Number;
        int index = (int)args.Table.Get(ArgLayer).Number;
        chara.overrideBodySprite = chara.FrameBySlot(spriteFrame);
    }

    // afterimage({enable?, count?, duration?});
    public void afterimage(DynValue args) {
        foreach (SpriteRenderer renderer in renderers) {
            AfterimageComponent imager = renderer.GetComponent<AfterimageComponent>();
            if (EnabledArg(args)) {
                float imageDuration = FloatArg(args, ArgDuration, 0.05f);
                int count = (int)FloatArg(args, ArgCount, 3);
                imager.enabled = true;
                imager.afterimageCount = count;
                imager.afterimageDuration = imageDuration;
            } else {
                imager.enabled = false;
            }
        }
    }

    // strike({power? duration?})
    public void strike(DynValue args) { CSRun(cs_strike(args), args); }
    private IEnumerator cs_strike(DynValue args) {
        float elapsed = 0.0f;
        float duration = FloatArg(args, ArgDuration, 0.4f);
        float power = FloatArg(args, ArgPower, 0.15f);
        Vector3 startPos = transform.localPosition;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            Vector3 offset = Vector3.Cross(chara.facing.Px(), new Vector3(0, 1, 0));
            offset = offset.normalized * Random.Range(-power, power);
            transform.localPosition = transform.localPosition + offset;
            yield return null;
        }
        transform.localPosition = startPos;
    }

    // bump({power? duration?})
    public void bump(DynValue args) { CSRun(cs_bump(args), args); }
    private IEnumerator cs_bump(DynValue args) {
        float duration = FloatArg(args, ArgDuration, 0.125f);
        float power = FloatArg(args, ArgPower, 0.3f);
        Vector3 facer = new Vector3(chara.facing.PxX(), chara.facing.PxY(), chara.facing.PxZ());
        Tweener t1 = transform.DOMove(transform.position + facer * power, duration / 2.0f);
        t1.SetEase(Ease.InBack);
        Tweener t2 = transform.DOMove(transform.position, duration / 2.0f);
        t2.SetEase(Ease.OutQuad);
        yield return CoUtils.RunSequence(new IEnumerator[] {
            CoUtils.RunTween(t1), CoUtils.RunTween(t2) });
    }

    // setItem({mode})
    public void setItem(DynValue args) {
        chara.itemMode = ItemModeExtensions.Parse(args.Table.Get(ArgMode).String);
    }

    // setArms({mode})
    public void setArms(DynValue args) {
        chara.armMode = ArmModeExtensions.Parse(args.Table.Get(ArgMode).String);
    }

    // animateSwing({duration=0.2f})
    public void animateSwing(DynValue args) { CSRun(cs_animateSwing(args), args); }
    private IEnumerator cs_animateSwing(DynValue args) {
        float duration = FloatArg(args, ArgDuration, 0.2f);
        yield return chara.itemLayer.GetComponent<SmearBehavior>().AnimateSlash(duration);
    }
}