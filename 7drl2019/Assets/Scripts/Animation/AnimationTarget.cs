using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

[MoonSharpUserData]
public class AnimationTarget : MonoBehaviour {

    protected static readonly string AnimPath = "Sprites/Anim/";
    protected static readonly string ArgDuration = "duration";
    protected static readonly string ArgName = "name";
    protected static readonly string ArgFrame = "frame";
    protected static readonly string ArgFrames = "frames";
    protected static readonly string ArgCount = "count";
    protected static readonly string ArgEnable = "enable";
    protected static readonly string ArgDisable = "disable";
    protected static readonly string ArgPower = "power";
    protected static readonly string ArgSpeed = "speed";
    protected static readonly string ArgRed = "r";
    protected static readonly string ArgBlue = "b";
    protected static readonly string ArgGreen = "g";
    protected static readonly string ArgAlpha = "a";
    protected static readonly float DefaultFrameDuration = 0.12f;

    public List<SpriteRenderer> renderers;

    protected Vector3 originalPos;

    [MoonSharpHidden]
    public void PrepareForAnimation() {
        originalPos = transform.position;
    }

    [MoonSharpHidden]
    public virtual void ResetAfterAnimation() {
        transform.position = originalPos;
    }

    // === COMMAND HELPERS =========================================================================

    protected void CSRun(IEnumerator routine, DynValue args) {
        StartCoroutine(routine);
    }

    protected float FloatArg(DynValue args, string argName, float defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : (float)value.Number;
        }
    }

    protected int IntArg(DynValue args, string argName, int defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : (int)value.Number;
        }
    }

    protected bool BoolArg(DynValue args, string argName, bool defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : value.Boolean;
        }
    }

    protected string StringArg(DynValue args, string argName, string defaultValue) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            DynValue value = args.Table.Get(argName);
            return (value == DynValue.Nil) ? defaultValue : value.String;
        }
    }

    protected bool EnabledArg(DynValue args, bool defaultValue = true) {
        if (args == DynValue.Nil || args == null || args.Table == null) {
            return defaultValue;
        } else {
            return BoolArg(args, ArgEnable, !BoolArg(args, ArgDisable, !defaultValue));
        }
    }

    protected IEnumerator ColorRoutine(DynValue args, float a, Func<Color> getColor, Action<Color> applyColor) {
        Color originalColor = getColor();
        float elapsed = 0.0f;
        float duration = FloatArg(args, ArgDuration, 0.0f);
        float speed = FloatArg(args, ArgSpeed, 0.2f);
        Vector3 startPos = transform.localPosition;
        float r = FloatArg(args, ArgRed, originalColor.r);
        float g = FloatArg(args, ArgGreen, originalColor.g);
        float b = FloatArg(args, ArgBlue, originalColor.b);

        while (elapsed < Mathf.Max(duration, speed)) {
            elapsed += Time.deltaTime;
            float t;
            if (duration > 0) {
                if (elapsed < speed) {
                    t = elapsed / speed;
                } else if (elapsed > (duration - speed)) {
                    t = (duration - elapsed) / speed;
                } else {
                    t = 1.0f;
                }
            } else {
                t = elapsed / speed;
            }

            Color color = new Color(
                Mathf.Lerp(originalColor.r, r, t),
                Mathf.Lerp(originalColor.g, g, t),
                Mathf.Lerp(originalColor.b, b, t),
                Mathf.Lerp(originalColor.a, a, t));
            applyColor(color);
            yield return null;
        }
        if (duration > 0) {
            applyColor(originalColor);
        }
    }

    // === LUA FUNCTIONS ===========================================================================

    // quake({power? duration?})
    public void quake(DynValue args) { CSRun(cs_quake(args), args); }
    private IEnumerator cs_quake(DynValue args) {
        float elapsed = 0.0f;
        float duration = FloatArg(args, ArgDuration, 0.25f);
        float power = FloatArg(args, ArgPower, 0.2f);
        MapCamera cam = Global.Instance().Maps.camera;
        Vector3 camPosition = cam.transform.localPosition;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            cam.transform.localPosition = new Vector3(
                    camPosition.x + UnityEngine.Random.Range(-power, power),
                    camPosition.y + UnityEngine.Random.Range(-power, power),
                    camPosition.z);
            yield return null;
        }
        cam.transform.localPosition = camPosition;
    }

    // tint({r, g, b, a?, duration?, speed?})
    public void tint(DynValue args) { CSRun(cs_tint(args), args); }
    private IEnumerator cs_tint(DynValue args) {
        float a = FloatArg(args, ArgAlpha, renderers[0].color.a);
        yield return ColorRoutine(args, a, () => {
            return renderers[0].color;
        }, (Color c) => {
            foreach (SpriteRenderer renderer in renderers) {
                renderer.color = c;
            }
        });
    }

    // flash({r, g, b, duration?, speed?, power?})
    public void flash(DynValue args) { CSRun(cs_flash(args), args); }
    private IEnumerator cs_flash(DynValue args) {
        float r = (float)args.Table.Get(ArgRed).Number;
        float g = (float)args.Table.Get(ArgGreen).Number;
        float b = (float)args.Table.Get(ArgBlue).Number;
        Color color = new Color(r, g, b, 0.0f);
        yield return ColorRoutine(args, FloatArg(args, ArgPower, 0.9f), () => {
            return color;
        }, (Color c) => {
            color = c;
            foreach (SpriteRenderer renderer in renderers) {
                renderer.material.SetColor("_Flash", c);
            }
        });
    }
}