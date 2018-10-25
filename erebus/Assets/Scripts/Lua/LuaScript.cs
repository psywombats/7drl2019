using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represents an runnable piece of Lua, usually from an event field
public class LuaScript {

    private DynValue function;

	public LuaScript(DynValue scriptFunction) {
        this.function = scriptFunction;
    }

    public void Run(Action callback = null) {
        Global.Instance().Lua.RunScript(function, callback);
    }

    public IEnumerator RunRoutine() {
        bool done = false;
        Run(() => {
            done = true;
        });
        while (!done) {
            yield return null;
        }
    }
}
