using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represents an eval-able piece of Lua, usually from an event field
public class LuaCondition {

    private DynValue function;

	public LuaCondition(DynValue scriptFunction) {
        this.function = scriptFunction;
    }

    public DynValue Evaluate() {
        return Global.Instance().Lua.Evaluate(function);
    }
}
