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

        // immediate functions
        globalContext.Globals["debugLog"] = (Action<DynValue>)DebugLog;

        // routines
        globalContext.Globals["cs_teleport"] = (Action<DynValue, DynValue, DynValue>)Teleport;
        globalContext.Globals["cs_showText"] = (Action<DynValue>)ShowText;
        globalContext.Globals["cs_hideTextbox"] = (Action)HideTextbox;
        globalContext.Globals["cs_wait"] = (Action<DynValue>)Wait;

        // global defines lua-side
        StreamReader reader = new StreamReader(DefinesPath);
        globalContext.DoStream(reader.BaseStream);
        reader.Close();
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
    public LuaRepresentation CreateObject() {
        return new LuaRepresentation(globalContext.DoString("return {}"));
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

    public DynValue Load(string luaChunk) {
        return globalContext.LoadString(luaChunk);
    }

    private IEnumerator ScriptRoutine(DynValue function) {
        Assert.IsNull(activeScript);
        activeScript = globalContext.CreateCoroutine(function).Coroutine;
        activeScript.Resume();
        while (activeScript.State != CoroutineState.Dead) {
            yield return null;
        }
    }

    private static void RunRoutineFromLua(IEnumerator routine) {
        Global.Instance().Lua.blockingRoutines += 1;
        Global.Instance().Lua.StartCoroutine(CoUtils.RunWithCallback(routine, Global.Instance().Lua, () => {
            Global.Instance().Lua.blockingRoutines -= 1;
            if (Global.Instance().Lua.blockingRoutines == 0) {
                Global.Instance().Lua.activeScript.Resume();
            }
        }));
    }

    private static void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }

    private static void Teleport(DynValue mapName, DynValue x, DynValue y) {
        RunRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, new IntVector2((int)x.Number, (int)y.Number)));
    }

    private static void ShowText(DynValue text) {
        RunRoutineFromLua(Textbox.GetInstance().ShowText(text.String));
    }

    private static void HideTextbox() {
        RunRoutineFromLua(Textbox.GetInstance().TransitionOut());
    }

    private static void Wait(DynValue seconds) {
        RunRoutineFromLua(CoUtils.Wait((float)seconds.Number));
    }
}
