using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System;

[MoonSharpUserData]
public class DollTargetEvent : TiledInstantiated {

    public enum Type {
        Attacker,
        Defender,
    }

    public Type type;

    [MoonSharpHidden]
    public override void Populate(IDictionary<string, string> properties) {
        switch (properties[MapEvent.PropertyTarget]) {
            case "attacker":
                this.type = Type.Attacker;
                break;
            case "defender":
                this.type = Type.Defender;
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    [MoonSharpHidden]
    public void ConfigureToBattler(BattleEvent battler) {
        GetComponent<CharaEvent>().SetAppearance(battler.GetComponent<CharaEvent>().GetAppearance());
    }

    // === LUA FUNCTIONS ===========================================================================
    
    private void debugLog(DynValue message) {
        Debug.Log(message.String);
    }
}
