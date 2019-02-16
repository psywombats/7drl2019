using UnityEngine;
using System.Collections;
using System;
using MoonSharp.Interpreter;

public class LuaCutsceneContext : LuaContext {

    private static readonly string DefinesPath = "Assets/Resources/Lua/cutscene_defines.lua";

    public override IEnumerator RunRoutine(LuaScript script) {
        if (Global.Instance().Maps.Avatar != null) {
            Global.Instance().Maps.Avatar.PauseInput();
        }
        yield return base.RunRoutine(script);
        if (MapOverlayUI.Instance().textbox.isDisplaying) {
            yield return MapOverlayUI.Instance().textbox.DisableRoutine();
        }
        if (Global.Instance().Maps.Avatar != null) {
            Global.Instance().Maps.Avatar.UnpauseInput();
        }
    }

    public void Start() {
        //lua.Globals["avatar"] = Global.Instance().Maps.Avatar.GetComponent<MapEvent>().luaObject;
    }

    public override void Awake() {
        base.Awake();
        LoadDefines(DefinesPath);
    }

    public override void RunRoutineFromLua(IEnumerator routine) {
        if (MapOverlayUI.Instance().textbox.isDisplaying) {
            routine = CoUtils.RunSequence(new IEnumerator[] {
                MapOverlayUI.Instance().textbox.DisableRoutine(),
                routine,
            });
        }

        base.RunRoutineFromLua(routine);
    }

    public void RunTextboxRoutineFromLua(IEnumerator routine) {
        base.RunRoutineFromLua(routine);
    }

    protected override void AssignGlobals() {
        base.AssignGlobals();
        lua.Globals["playBGM"] = (Action<DynValue>)PlayBGM;
        lua.Globals["cs_teleportCoords"] = (Action<DynValue, DynValue, DynValue>)Teleport;
        lua.Globals["cs_teleport"] = (Action<DynValue, DynValue>)Teleport;
        lua.Globals["cs_fadeOutBGM"] = (Action<DynValue>)FadeOutBGM;
        lua.Globals["cs_speak"] = (Action<DynValue, DynValue>)Speak;
    }

    // === LUA CALLABLE ============================================================================

    private void PlayBGM(DynValue bgmKey) {
        Global.Instance().Audio.PlayBGM(bgmKey.String);
    }

    private void Teleport(DynValue mapName, DynValue x, DynValue y) {
        RunRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, new IntVector2((int)x.Number, (int)y.Number)));
    }

    private void Teleport(DynValue mapName, DynValue targetEventName) {
        RunRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, targetEventName.String));
    }

    private void FadeOutBGM(DynValue seconds) {
        RunRoutineFromLua(Global.Instance().Audio.FadeOutRoutine((float)seconds.Number));
    }

    private void Speak(DynValue speaker, DynValue text) {
        RunTextboxRoutineFromLua(MapOverlayUI.Instance().textbox.SpeakRoutine(speaker.String, text.String));
    }
}
