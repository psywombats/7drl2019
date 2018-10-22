using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DollTargetEvent : TiledInstantiated {

    public enum Type {
        Attacker,
        Defender,
    }

    public Type type;

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

    public void ConfigureToBattler(BattleEvent battler) {
        GetComponent<CharaEvent>().SetAppearance(battler.GetComponent<CharaEvent>().GetAppearance());
    }
}
