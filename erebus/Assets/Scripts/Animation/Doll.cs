using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[MoonSharpUserData]
[RequireComponent(typeof(AfterimageComponent))]
public class Doll : AnimationTarget {

    private static float DefaultJumpHeight = 1.2f;
    private static float DefaultJumpReturnHeight = 0.4f;

    public enum Type {
        Attacker,
        Defender,
    }

    public Type type;
    public BattleAnimationPlayer player;

    private Vector3 originalDollPos;

    private CharaAnimator _animator;
    public CharaAnimator animator {
        get {
            if (_animator == null) {
                _animator = GetComponent<CharaAnimator>();
            }
            return _animator;
        }
    }

    [MoonSharpHidden]
    public void ConfigureToBattler(BattleEvent battler) {
        GetComponent<CharaEvent>().SetAppearance(battler.GetComponent<CharaEvent>().GetAppearance());
    }

    [MoonSharpHidden]
    public void PrepareForAnimation(BattleAnimationPlayer player) {
        this.player = player;
        animator.PrepareForAnimation();
        originalDollPos = transform.position;
    }

    [MoonSharpHidden]
    public override void ResetAfterAnimation() {
        animator.ResetAfterAnimation();
        transform.position = originalDollPos;
        GetComponent<AfterimageComponent>().enabled = false;
    }

    // === COMMAND HELPERS =========================================================================
    
    
    private Vector3 CalculateJumpOffset(Vector3 startPos, Vector3 endPos) {
        Vector3 dir = (endPos - startPos).normalized;
        return endPos - 0.75f * dir;
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
        Vector3 endPos = CalculateJumpOffset(transform.position, player.defender.transform.position);
        float duration = (float)args.Table.Get(ArgDuration).Number;
        yield return JumpRoutine(endPos, duration, DefaultJumpHeight);
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

    // setFrame({sheet, frame});
    public void setFrame(DynValue args) {
        string spriteName = args.Table.Get(ArgSpritesheet).String;
        int spriteFrame = (int)args.Table.Get(ArgFrame).Number;
        Sprite[] sprites = Resources.LoadAll<Sprite>(AnimPath + spriteName);
        Sprite sprite = sprites[spriteFrame];
        animator.SetOverrideSprite(sprite);
    }

    // setAnim({sheet, frame[]}, duration?);
    public void setAnim(DynValue args) {
        string spriteName = args.Table.Get(ArgSpritesheet).String;
        float frameDuration = FloatArg(args, ArgDuration, DefaultFrameDuration);
        Sprite[] sprites = Resources.LoadAll<Sprite>(AnimPath + spriteName);
        List<Sprite> frames = new List<Sprite>();
        foreach (DynValue value in args.Table.Get(ArgFrames).Table.Values) {
            frames.Add(sprites[(int)value.Number]);
        }
        animator.SetOverrideAnim(frames, frameDuration);
    }

    // afterimage({enable?, count?, duration?});
    public void afterimage(DynValue args) {
        AfterimageComponent imager = GetComponent<AfterimageComponent>();
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

    // strike({power? duration?})
    public void strike(DynValue args) { CSRun(cs_strike(args), args); }
    private IEnumerator cs_strike(DynValue args) {
        float elapsed = 0.0f;
        float duration = FloatArg(args, ArgDuration, 0.4f);
        float power = FloatArg(args, ArgPower, 0.1f);
        Vector3 startPos = transform.localPosition;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            transform.localPosition = new Vector3(
                    startPos.x + UnityEngine.Random.Range(-power, power),
                    startPos.y,
                    startPos.z);
            yield return null;
        }
        transform.localPosition = startPos;
    }
}
