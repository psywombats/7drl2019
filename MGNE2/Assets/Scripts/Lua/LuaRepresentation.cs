using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaRepresentation {

    private DynValue value;

    public LuaRepresentation(DynValue value) {
        this.value = value;
    }

    // meant to be called with the key/value of a lualike property on a Tiled object
    // accepts nil and zero-length as no-ops
    public void Set(string name, string luaChunk) {
        if (luaChunk != null && luaChunk.Length > 0) {
            value.Table.Set(name, Global.Instance().Lua.Load(luaChunk));
        }
    }

    public void Run(string eventName, Action callback = null) {
        DynValue function = value.Table.Get(eventName);
        if (function == DynValue.Nil) {
            if (callback != null) {
                callback();
            }
        } else {
            Global.Instance().Lua.RunScript(function, callback);
        }
    }

    public DynValue Evaluate(string propertyName) {
        DynValue function = value.Table.Get(propertyName);
        if (function == DynValue.Nil) {
            return DynValue.Nil;
        } else {
            return Global.Instance().Lua.Evaluate(function);
        }
    }

    public bool EvaluateBool(string propertyName, bool defaultValue = false) {
        DynValue result = Evaluate(propertyName);
        if (result == DynValue.Nil) {
            return defaultValue;
        } else {
            return result.Boolean;
        }
    }
}
