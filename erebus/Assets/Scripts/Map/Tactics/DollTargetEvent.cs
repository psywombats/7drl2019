using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System;

[MoonSharpUserData]
public class DollTargetEvent : TiledInstantiated {

    private static string AnimPath = "Sprites/Anim/";
    private static string ArgDuration = "duration";
    private static string ArgSpritesheet = "sheet";
    private static string ArgFrame = "frame";
    private static string ArgFrames = "frames";
    private static string ArgCount = "count";
    private static string ArgEnable = "enable";
    private static string ArgDisable = "disable";

    private static float DefaultFrameDuration = 0.12f;
    private static float DefaultJumpHeight = 1.2f;
    private static float DefaultJumpReturnHeight = 0.4f;
    private static float DefaultAfterimageDuration = 0.075f;
    private static int DefaultAfterimageCount = 2;

    public enum Type {
        Attacker,
        Defender,
    }

    public Type type;
    public BattleAnimationPlayer player;

    private Vector3 originalDollPos;

    private MapEvent _mapEvent;
    public MapEvent mapEvent {
        get {
            if (_mapEvent == null) {
                _mapEvent = GetComponent<MapEvent>();
            }
            return _mapEvent;
        }
    }

    private GameObject _doll;
    public GameObject doll {
        get {
            if (_doll == null) {
                _doll = mapEvent.GetComponent<CharaEvent>().doll;
            }
            return _doll;
        }
    }

    private CharaAnimator _animator;
    public CharaAnimator animator {
        get {
            if (_animator == null) {
                _animator = doll.GetComponent<CharaAnimator>();
            }
            return _animator;
        }
    }

    [MoonSharpHidden]
    public override void Populate(IDictionary<string, string> properties) {
        switch (properties[MapEvent.PropertyTarget]) {
            case "attacker":
                this.type = Type.Attacker;
                break;
            case "defender":
                this.type = Type.Defender;
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    [MoonSharpHidden]
    public void ConfigureToBattler(BattleEvent battler) {
        GetComponent<CharaEvent>().SetAppearance(battler.GetComponent<CharaEvent>().GetAppearance());
    }

    [MoonSharpHidden]
    public void PrepareForAnimation() {
        animator.PrepareForAnimation();
        originalDollPos = doll.transform.position;
    }

    [MoonSharpHidden]
    public void ResetAfterAnimation() {
        animator.ResetAfterAnimation();
        doll.transform.position = originalDollPos;
        doll.GetComponent<AfterimageComponent>().enabled = false;
    }

    [MoonSharpHidden]
    private void CSRun(IEnumerator routine, DynValue args) {
        StartCoroutine(routine);
    }

    [MoonSharpHidden]
    private Vector3 CalculateJumpOffset(Vector3 startPos, Vector3 endPos) {
        Vector3 dir = (endPos - startPos).normalized;
        return endPos - 0.75f * dir;
    }
    
    [MoonSharpHidden]
    private IEnumerator JumpRoutine(Vector3 endPos, float duration, float height) {
        Vector3 startPos = doll.transform.position;
        float elapsed = 0.0f;
        while (doll.transform.position != endPos) {
            elapsed += Time.deltaTime;
            Vector3 lerped = Vector3.Lerp(startPos, endPos, elapsed / duration);
            doll.transform.position = new Vector3(
                    lerped.x,
                    lerped.y + ((elapsed >= duration) 
                            ? 0 
                            : Mathf.Sin(elapsed / duration * Mathf.PI) * height),
                    lerped.z);
            yield return null;
        }
    }

    [MoonSharpHidden]
    private float FloatArg(DynValue args, string argName, float defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : (float)value.Number;
        }
    }

    [MoonSharpHidden]
    private bool BoolArg(DynValue args, string argName, bool defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : value.Boolean;
        }
    }

    [MoonSharpHidden]
    private bool EnabledArg(DynValue args, bool defaultValue = true) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            return BoolArg(args, ArgEnable, !BoolArg(args, ArgDisable, !defaultValue));
        }
    }

    // === LUA FUNCTIONS ===========================================================================

    public void jumpToDefender(DynValue args) { CSRun(cs_jumpToDefender(args), args); }
    [MoonSharpHidden] IEnumerator cs_jumpToDefender(DynValue args) {
        Vector3 endPos = CalculateJumpOffset(doll.transform.position, player.defender.doll.transform.position);
        float duration = (float)args.Table.Get(ArgDuration).Number;
        yield return JumpRoutine(endPos, duration, DefaultJumpHeight);
    }

    public void jumpReturn(DynValue args) { CSRun(cs_jumpReturn(args), args); }
    [MoonSharpHidden] IEnumerator cs_jumpReturn(DynValue args) {
        float overallDuration = (float)args.Table.Get(ArgDuration).Number;
        float fraction = (2.0f / 3.0f);
        Vector3 midPos = Vector3.Lerp(doll.transform.position, originalDollPos, fraction);
        yield return JumpRoutine(midPos, 
                    overallDuration * fraction, 
                    DefaultJumpReturnHeight * fraction);
        yield return JumpRoutine(originalDollPos, 
                overallDuration * (1.0f - fraction) * 1.5f, 
                DefaultJumpReturnHeight * (1.0f - fraction));
    }

    public void setFrame(DynValue args) {
        string spriteName = args.Table.Get(ArgSpritesheet).String;
        int spriteFrame = (int)args.Table.Get(ArgFrame).Number;
        Sprite[] sprites = Resources.LoadAll<Sprite>(AnimPath + spriteName);
        Sprite sprite = sprites[spriteFrame];
        animator.SetOverrideSprite(sprite);
    }

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

    public void afterimage(DynValue args) {
        AfterimageComponent imager = doll.GetComponent<AfterimageComponent>();
        if (EnabledArg(args)) {
            float imageDuration = FloatArg(args, ArgDuration, DefaultAfterimageDuration);
            int count = (int)FloatArg(args, ArgCount, DefaultAfterimageCount);
            imager.enabled = true;
            imager.afterimageCount = count;
            imager.afterimageDuration = imageDuration;
        } else {
            imager.enabled = false;
        }
    }
}
