using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BattleEvent))]
public class PCEvent : MonoBehaviour {

    public MapEvent parent { get { return GetComponent<MapEvent>(); } }
    public BattleController battle { get { return GetComponent<BattleEvent>().unit.battle; } }
    public BattleUnit unit { get { return GetComponent<BattleEvent>().unit; } }

    public Inventory inventory { get; private set; }

    public void Start() {
        Global.Instance().Maps.pc = this;
        inventory = new Inventory();
    }
}
