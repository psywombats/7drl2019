using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestEvent : TiledInstantiated {

    private string itemKey;

    public override void Populate(IDictionary<string, string> properties) {
        itemKey = properties["item"];
    }

    public void Start() {
        GetComponent<Dispatch>().RegisterListener(MapEvent.EventCollide, (object payload) => {
            OnCollide((AvatarEvent)payload);
        });
    }

    private void OnCollide(AvatarEvent avatar) {

    }
}
