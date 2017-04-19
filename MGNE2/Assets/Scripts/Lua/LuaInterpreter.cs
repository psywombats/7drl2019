using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.IO;

public class LuaInterpreter : MonoBehaviour {

    private static readonly string DefinesPath = "Assets/Resources/Scenes/Defines.lua";

    private Script globalContext;
    
    public void Awake() {
        globalContext = new Script();

        // immediate functions
        globalContext.Globals["debugLog"] = (Action<DynValue>)DebugLog;
        globalContext.Globals["teleport"] = (Action<DynValue, DynValue, DynValue>)Teleport;

        // routines
        globalContext.Globals["cs_speak"] = (Func<DynValue, IEnumerator>)Speak;
        globalContext.Globals["cs_hideText"] = (Func<IEnumerator>)HideText;
        globalContext.Globals["cs_wait"] = (Func<DynValue, IEnumerator>)Wait;

        // global defines lua-side
        StreamReader reader = new StreamReader(DefinesPath);
        globalContext.DoStream(reader.BaseStream);
        reader.Close();
    }

    // generates a lua script object from the specified lua guts
    public LuaScript CreateScript(string luaScript) {
        luaScript = "return function()\n" + luaScript;
        luaScript = luaScript + "\nend";
        return new LuaScript(globalContext.DoString(luaScript));
    }

    public LuaCondition CreateCondition(string luaScript) {
        return new LuaCondition(globalContext.LoadString(luaScript));
    }

    // evaluates a lua function in the global context
    public DynValue Evaluate(DynValue function) {
        return globalContext.Call(function);
    }

    // executes asynchronously, for cutscenes
    public void RunScript(DynValue function, Action callback = null) {
        StartCoroutine(CoUtils.RunWithCallback(RunRoutine(function), this, () => {
            if (callback != null) {
                callback();
            }
        }));
    }

    private IEnumerator RunRoutine(DynValue function) {
        DynValue coroutine = globalContext.CreateCoroutine(function);
        while (coroutine.Coroutine.State != CoroutineState.Dead) {
            coroutine.Coroutine.Resume();
            yield return null;
        }
    }

    private IEnumerator InstancedWaitForRoutine(IEnumerator routine) {
        bool finished = false;
        StartCoroutine(CoUtils.RunWithCallback(routine, this, () => {
            finished = true;
        }));
        while (!finished) {
            yield return null;
        }
    }

    private static IEnumerator WaitForRoutine(IEnumerator routine) {
        return Global.Instance().Lua.InstancedWaitForRoutine(routine);
    }

    private static void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }

    private static void Teleport(DynValue mapName, DynValue x, DynValue y) {
        Global.Instance().Maps.Teleport(mapName.String, new IntVector2((int)x.Number, (int)y.Number));
    }

    private static IEnumerator Speak(DynValue text) {
        return WaitForRoutine(Textbox.GetInstance().ShowText(text.String));
    }

    private static IEnumerator HideText() {
        return Textbox.GetInstance().TransitionOut();
    }

    private static IEnumerator Wait(DynValue seconds) {
        return WaitForRoutine(UnityWait((float)seconds.Number));
    }

    private static IEnumerator UnityWait(float seconds) {
        yield return new WaitForSeconds(seconds);
    }
}
