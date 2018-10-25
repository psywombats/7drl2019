using UnityEngine;
using System.Collections;
using System;

/**
 * A serialized battle animation is essentially a just a lua script, meant to execute in the context
 * of a battle animation player. Not sure it really needs anything fancier than that -- an actual
 * frame by frame editor like RM's might be need but is outside scope for now.
 */
[CreateAssetMenu(fileName = "BattleAnim", menuName = "Data/RPG/BattleAnim")]
public class BattleAnimation : ScriptableObject {

    [TextArea(6, 24)]
    public string script;

    public LuaScript ToScript() {
        return Global.Instance().Lua.CreateScript(script);
    }

}
