using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System;

[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class DollTargetEvent : TiledInstantiated {

    public Doll doll;

    private MapEvent _mapEvent;
    public MapEvent mapEvent {
        get {
            if (_mapEvent == null) {
                _mapEvent = GetComponent<MapEvent>();
            }
            return _mapEvent;
        }
    }

    public override void Populate(IDictionary<string, string> properties) {
        GetComponent<CharaEvent>().gameObject.AddComponent<Doll>();
        gameObject.AddComponent<AfterimageComponent>();
        switch (properties[MapEvent.PropertyTarget]) {
            case "attacker":
                GetComponent<Doll>().type = Doll.Type.Attacker;
                break;
            case "defender":
                GetComponent<Doll>().type = Doll.Type.Defender;
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }
}
