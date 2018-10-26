using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System;

[MoonSharpUserData]
public class DollTargetEvent : TiledInstantiated {

    private static string ArgDuration = "duration";

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
    }

    [MoonSharpHidden]
    private void CSRun(IEnumerator routine, DynValue args) {
        StartCoroutine(routine);
    }

    [MoonSharpHidden]
    private Vector3 CalculateJumpOffset(Vector3 startPos, Vector3 endPos) {
        Vector3 dir = (endPos - startPos).normalized;
        return endPos - 0.5f * dir;
    }

    // === LUA FUNCTIONS ===========================================================================

    public void jumpToDefender(DynValue args) { CSRun(cs_jumpToDefender(args), args); }
    [MoonSharpHidden] IEnumerator cs_jumpToDefender(DynValue args) {
        Vector3 startPos = doll.transform.position;
        Vector3 endPos = CalculateJumpOffset(startPos, player.defender.doll.transform.position);
        float duration = (float)args.Table.Get(ArgDuration).Number;
        float elapsed = 0.0f;
        while (doll.transform.position != endPos) {
            elapsed += Time.deltaTime;
            Vector3 lerped = Vector3.Lerp(startPos, endPos, elapsed / duration);
            doll.transform.position = new Vector3(
                    lerped.x,
                    lerped.y + ((elapsed >= duration) ? 0 : Mathf.Sin(elapsed / duration * Mathf.PI) * 1.2f),
                    lerped.z);
            yield return null;
        }
    }
}
