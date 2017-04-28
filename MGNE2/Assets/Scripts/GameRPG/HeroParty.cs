using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroParty : Party {

    public PartyInventory Inventory { get; private set; }
    
    public HeroParty() {
        Inventory = new PartyInventory();
    }
}
