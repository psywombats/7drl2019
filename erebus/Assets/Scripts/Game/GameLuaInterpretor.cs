using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameLuaInterpretor : MonoBehaviour {

    private static readonly string DefinesPath = "Assets/Resources/Scenes/GameDefines.lua";

    private LuaInterpreter lua;

    public void Start() {
        lua = Global.Instance().Lua;

        lua.LoadDefines(DefinesPath);

        // immediate functions
        lua.GlobalContext.Globals["addItem"] = (Action<DynValue>)AddItem;
        lua.GlobalContext.Globals["deductItem"] = (Action<DynValue>)DeductItem;
        lua.GlobalContext.Globals["hasItem"] = (Func<DynValue, DynValue>)HasItem;
    }

    private static void AddItem(DynValue itemName) {
        ItemData item = ItemData.ItemByName(itemName.String);
        Assert.IsNotNull(item);
        GGlobal.Instance().Party.Inventory.AddItem(item);
    }

    private static void DeductItem(DynValue itemName) {
        ItemData item = ItemData.ItemByName(itemName.String);
        Assert.IsNotNull(item);
        GGlobal.Instance().Party.Inventory.DeductItem(item);
    }

    private static DynValue HasItem(DynValue itemName) {
        ItemData item = ItemData.ItemByName(itemName.String);
        Assert.IsNotNull(item);
        return Global.Instance().Lua.Marshal(GGlobal.Instance().Party.Inventory.HasItem(item));
    }
}
