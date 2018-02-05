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
        //lua.GlobalContext.Globals["addItem"] = (Action<DynValue>)AddItem;
    }

    //private static void AddItem(DynValue itemName) {
    //    ItemData item = ItemData.ItemByName(itemName.String);
    //    Assert.IsNotNull(item);
    //    GGlobal.Instance().Party.Inventory.AddItem(item);
    //}
}
