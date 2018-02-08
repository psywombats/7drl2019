using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaCommand : SceneCommand {

    string luaChunk;

    public LuaCommand(string luaChunk) {
        this.luaChunk = luaChunk;
    }

    public override IEnumerator PerformAction() {
        LuaScript script = Global.Instance().Lua.CreateScript(luaChunk);

        bool finished = false;
        script.Run(() => {
            finished = true;
        });
        while (!finished) {
            yield break;
        }
    }
}
