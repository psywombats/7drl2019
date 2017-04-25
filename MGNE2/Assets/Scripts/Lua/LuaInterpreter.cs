using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using MoonSharp.Interpreter;
using System;
using System.IO;

public class LuaInterpreter : MonoBehaviour {

    private static readonly string DefinesPath = "Assets/Resources/Scenes/Defines.lua";

    private Script globalContext;

    private MoonSharp.Interpreter.Coroutine activeScript;
    private int blockingRoutines;
    
    public void Awake() {
        globalContext = new Script();
        UserData.RegisterAssembly();

        // immediate functions
        globalContext.Globals["debugLog"] = (Action<DynValue>)DebugLog;
        globalContext.Globals["getSwitch"] = (Func<DynValue, DynValue>)GetSwitch;
        globalContext.Globals["setSwitch"] = (Action<DynValue, DynValue>)SetSwitch;
        globalContext.Globals["eventNamed"] = (Func<DynValue, LuaMapEvent>)EventNamed;
        globalContext.Globals["playSFX"] = (Action<DynValue>)PlaySFX;

        // routines
        globalContext.Globals["cs_teleport"] = (Action<DynValue, DynValue, DynValue>)Teleport;
        globalContext.Globals["cs_teleportTarget"] = (Action<DynValue, DynValue>)Teleport;
        globalContext.Globals["cs_showText"] = (Action<DynValue>)ShowText;
        globalContext.Globals["cs_hideTextbox"] = (Action)HideTextbox;
        globalContext.Globals["cs_wait"] = (Action<DynValue>)Wait;

        // global defines lua-side
        StreamReader reader = new StreamReader(DefinesPath);
        globalContext.DoStream(reader.BaseStream);
        reader.Close();
    }

    public void RegisterAvatar(AvatarEvent avatar) {
        globalContext.Globals["avatar"] = avatar.GetComponent<MapEvent>().LuaObject;
    }

    // generates a lua script object from the specified lua guts, can be run as a process
    public LuaScript CreateScript(string luaScript) {
        luaScript = "return function()\n" + luaScript;
        luaScript = luaScript + "\nend";
        return new LuaScript(globalContext.DoString(luaScript));
    }

    // meant to be evaluated synchronously
    public LuaCondition CreateCondition(string luaScript) {
        return new LuaCondition(globalContext.LoadString(luaScript));
    }

    // creates an empty table as the lua representation of some c# object
    public LuaMapEvent CreateEvent(MapEvent mapEvent) {
        return new LuaMapEvent(globalContext.DoString("return {}"), mapEvent);
    }

    // evaluates a lua function in the global context
    public DynValue Evaluate(DynValue function) {
        return globalContext.Call(function);
    }

    // executes asynchronously, for cutscenes
    public void RunScript(DynValue function, Action callback = null) {
        Global.Instance().Maps.Avatar.InputPaused = true;
        StartCoroutine(CoUtils.RunWithCallback(ScriptRoutine(function), this, () => {
            Global.Instance().Maps.Avatar.InputPaused = false;
            activeScript = null;
            if (callback != null) {
                callback();
            }
        }));
    }

    // hang on to a chunk of lua to run later
    public DynValue Load(string luaChunk) {
        return globalContext.LoadString(luaChunk);
    }

    // call a coroutine from lua code
    // any coroutines invoked by proxy objects need to run here
    public void RunRoutineFromLua(IEnumerator routine) {
        Assert.IsNotNull(activeScript);
        blockingRoutines += 1;
        StartCoroutine(CoUtils.RunWithCallback(routine, Global.Instance().Lua, () => {
            blockingRoutines -= 1;
            if (blockingRoutines == 0 && activeScript != null) {
                activeScript.Resume();
            }
        }));
    }

    private IEnumerator ScriptRoutine(DynValue function) {
        Assert.IsNull(activeScript);
        activeScript = globalContext.CreateCoroutine(function).Coroutine;
        activeScript.Resume();
        while (activeScript.State != CoroutineState.Dead) {
            yield return null;
        }
    }

    private static DynValue Marshal(object toMarshal) {
        return DynValue.FromObject(Global.Instance().Lua.globalContext, toMarshal);
    }

    private static void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }

    private static DynValue GetSwitch(DynValue switchName) {
        bool value = Global.Instance().Memory.GetSwitch(switchName.String);
        return Marshal(value);
    }

    private static void SetSwitch(DynValue switchName, DynValue value) {
        Global.Instance().Memory.SetSwitch(switchName.String, value.Boolean);
    }

    private static LuaMapEvent EventNamed(DynValue eventName) {
        MapEvent mapEvent = Global.Instance().Maps.ActiveMap.GetEventNamed(eventName.String);
        if (mapEvent == null) {
            return null;
        } else {
            return mapEvent.LuaObject;
        }
    }

    private static void PlaySFX(DynValue sfxKey) {
        Global.Instance().Audio.PlaySFX(sfxKey.String);
    }

    // Routines

    private static void RunStaticRoutineFromLua(IEnumerator routine) {
        Global.Instance().Lua.RunRoutineFromLua(routine);
    }

    private static void Teleport(DynValue mapName, DynValue x, DynValue y) {
        RunStaticRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, new IntVector2((int)x.Number, (int)y.Number)));
    }

    private static void Teleport(DynValue mapName, DynValue targetEventName) {
        RunStaticRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, targetEventName.String));
    }

    private static void ShowText(DynValue text) {
        RunStaticRoutineFromLua(Textbox.GetInstance().ShowText(text.String));
    }

    private static void HideTextbox() {
        RunStaticRoutineFromLua(Textbox.GetInstance().TransitionOut());
    }

    private static void Wait(DynValue seconds) {
        RunStaticRoutineFromLua(CoUtils.Wait((float)seconds.Number));
    }
}
