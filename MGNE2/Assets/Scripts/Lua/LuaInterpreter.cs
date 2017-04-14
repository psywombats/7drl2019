using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System;

public class LuaInterpreter : MonoBehaviour {

    private Script globalContext;
    
    public void Awake() {
        globalContext = new Script();

        // function definitions!
        globalContext.Globals["debugLog"] = (Action<DynValue>)DebugLog;
    }

    // generates a lua script object from the specified lua guts
    // the lua provided should be repl-style, like the body of a main (will be eval'd)
    public LuaChunk CreateChunk(string luaScript) {
        return new LuaChunk(globalContext.LoadString(luaScript));
    }

    // evaluates a lua function in the global context
    public DynValue Run(DynValue function) {
        return globalContext.Call(function);
    }

    private static void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }
}
