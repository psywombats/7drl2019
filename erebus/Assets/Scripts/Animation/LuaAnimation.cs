using UnityEngine;
using System.Collections;
using System;

/**
 * A serialized battle animation is essentially a just a lua script, meant to execute in the context
 * of a battle animation player. Not sure it really needs anything fancier than that -- an actual
 * frame by frame editor like RM's might be need but is outside scope for now.
 */
[CreateAssetMenu(fileName = "LuaAnimation", menuName = "Data/LuaAnimation")]
public class LuaAnimation : ScriptableObject {

    [TextArea(6, 24)]
    public string script;

    public LuaScript ToScript(LuaContext context) {
        return new LuaScript(context, script);
    }
}
