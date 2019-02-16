using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using Coroutine = MoonSharp.Interpreter.Coroutine;
using System;
using UnityEngine.Assertions;
using System.IO;

// a wrapper around Script that represents an environment where a script can execute
public class LuaContext : MonoBehaviour {

    private static readonly string DefinesPath = "Assets/Resources/Lua/global_defines.lua";
    private static string defines;

    private Script _lua;
    public Script lua {
        get {
            if (_lua == null) {
                _lua = new Script();
            }
            return _lua;
        }
    }

    private LuaScript activeScript;
    private int blockingRoutines;

    public virtual void Awake() {
        LoadDefines(DefinesPath);
        AssignGlobals();
    }

    // make sure the luaobject has been registered via [MoonSharpUserData]
    public void SetGlobal(string key, object luaObject) {
        lua.Globals[key] = luaObject;
    }

    public DynValue CreateObject() {
        return lua.DoString("return {}");
    }

    public DynValue Marshal(object toMarshal) {
        return DynValue.FromObject(lua, toMarshal);
    }

    public Coroutine CreateScript(string fullScript) {
        try {
            DynValue scriptFunction = lua.DoString(fullScript);
            return lua.CreateCoroutine(scriptFunction).Coroutine;
        } catch (SyntaxErrorException e) {
            Debug.LogError("bad script: " + fullScript);
            throw e;
        }
    }

    // all coroutines that are meant to block execution of the script should go through here
    public virtual void RunRoutineFromLua(IEnumerator routine) {
        blockingRoutines += 1;
        StartCoroutine(CoUtils.RunWithCallback(routine, () => {
            blockingRoutines -= 1;
            if (blockingRoutines == 0) {
                activeScript.scriptRoutine.Resume();
            }
        }));
    }

    // meant to be evaluated synchronously
    public LuaCondition CreateCondition(string luaScript) {
        return new LuaCondition(this, lua.LoadString(luaScript));
    }

    // evaluates a lua function in the global context
    public DynValue Evaluate(DynValue function) {
        return lua.Call(function);
    }

    // hang on to a chunk of lua to run later
    public DynValue Load(string luaChunk) {
        return lua.LoadString(luaChunk);
    }

    public virtual IEnumerator RunRoutine(LuaScript script) {
        Assert.IsNull(activeScript);
        activeScript = script;
        try {
            script.scriptRoutine.Resume();
        } catch (Exception e) {
            Debug.Log("Exception during script: " + script);
            throw e;
        }
        while (script.scriptRoutine.State != CoroutineState.Dead) {
            yield return null;
        }
        activeScript = null;
    }

    protected virtual void AssignGlobals() {
        lua.Globals["debugLog"] = (Action<DynValue>)DebugLog;
        lua.Globals["playSFX"] = (Action<DynValue>)PlaySFX;
        lua.Globals["cs_wait"] = (Action<DynValue>)Wait;
        lua.Globals["getSwitch"] = (Func<DynValue, DynValue>)GetSwitch;
        lua.Globals["setSwitch"] = (Action<DynValue, DynValue>)SetSwitch;
        lua.Globals["eventNamed"] = (Func<DynValue, LuaMapEvent>)EventNamed;
    }

    protected void LoadDefines(string path) {
        StreamReader reader = new StreamReader(path);
        lua.DoStream(reader.BaseStream);
        reader.Close();
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

    private void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }

    private void Wait(DynValue seconds) {
        RunRoutineFromLua(CoUtils.Wait((float)seconds.Number));
    }

    private void PlaySFX(DynValue sfxKey) {
        Global.Instance().Audio.PlaySFX(sfxKey.String);
    }
}
