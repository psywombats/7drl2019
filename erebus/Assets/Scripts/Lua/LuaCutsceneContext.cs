using UnityEngine;
using System.Collections;
using System;
using MoonSharp.Interpreter;

public class LuaCutsceneContext : LuaContext {

    private static readonly string DefinesPath = "Assets/Resources/Scripts/cutscene_defines.lua";
    private static string cutsceneDefines;

    public LuaCutsceneContext() : base() {
        if (cutsceneDefines == null) {
            TextAsset luaText = Resources.Load<TextAsset>(DefinesPath);
            cutsceneDefines = luaText.text;
        }
        lua.DoString(cutsceneDefines);
    }

    public override IEnumerator RunRoutine(LuaScript script) {
        if (Global.Instance().Maps.Avatar != null) {
            Global.Instance().Maps.Avatar.PauseInput();
        }
        yield return base.RunRoutine(script);
        if (Global.Instance().Maps.Avatar != null) {
            Global.Instance().Maps.Avatar.UnpauseInput();
        }
    }

    public void Start() {
        lua.Globals["avatar"] = Global.Instance().Maps.Avatar.GetComponent<MapEvent>().luaObject;
    }

    protected override void AssignGlobals() {
        lua.Globals["getSwitch"] = (Func<DynValue, DynValue>)GetSwitch;
        lua.Globals["setSwitch"] = (Action<DynValue, DynValue>)SetSwitch;
        lua.Globals["eventNamed"] = (Func<DynValue, LuaMapEvent>)EventNamed;
        lua.Globals["playBGM"] = (Action<DynValue>)PlayBGM;
        lua.Globals["cs_teleportCoords"] = (Action<DynValue, DynValue, DynValue>)Teleport;
        lua.Globals["cs_teleport"] = (Action<DynValue, DynValue>)Teleport;
        lua.Globals["cs_fadeOutBGM"] = (Action<DynValue>)FadeOutBGM;
    }

    // === LUA CALLABLE ============================================================================

    private LuaMapEvent EventNamed(DynValue eventName) {
        MapEvent mapEvent = Global.Instance().Maps.ActiveMap.GetEventNamed(eventName.String);
        if (mapEvent == null) {
            return null;
        } else {
            return mapEvent.luaObject;
        }
    }

    private DynValue GetSwitch(DynValue switchName) {
        bool value = Global.Instance().Memory.GetSwitch(switchName.String);
        return Marshal(value);
    }

    private void SetSwitch(DynValue switchName, DynValue value) {
        Global.Instance().Memory.SetSwitch(switchName.String, value.Boolean);
    }

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
}
