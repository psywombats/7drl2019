using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class DollTargetEvent : TiledInstantiated {

    public Doll doll;

    public override void Populate(IDictionary<string, string> properties) {
        doll = GetComponent<CharaEvent>().doll.AddComponent<Doll>();
        doll.gameObject.AddComponent<AfterimageComponent>();
        doll.appearance = GetComponent<CharaEvent>().doll.GetComponent<SpriteRenderer>();
        switch (properties[MapEvent.PropertyTarget]) {
            case "attacker":
                doll.type = Doll.Type.Attacker;
                break;
            case "defender":
                doll.type = Doll.Type.Defender;
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }
}
