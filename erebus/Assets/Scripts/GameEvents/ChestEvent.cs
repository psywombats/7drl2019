using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventAppearance))]
[RequireComponent(typeof(MapEvent))]
public class ChestEvent : TiledInstantiated {

    private const string OpenSpriteName = "Events/chest_open";
    private const string ClosedSpriteName = "Events/chest_closed";

    public string ItemKey;

    public bool Opened {
        get { return Global.Instance().Memory.GetSwitch(GetSwitchName()); }
        set { Global.Instance().Memory.SetSwitch(GetSwitchName(), true); }
    }

    public override void Populate(IDictionary<string, string> properties) {
        ItemKey = properties["item"];
        GetComponent<EventAppearance>().SetAppearance(ClosedSpriteName);
        GetComponent<MapEvent>().Passable = false;
    }

    public void Start() {
        GetComponent<Dispatch>().RegisterListener(MapEvent.EventInteract, (object payload) => {
            OnInteract((AvatarEvent)payload);
        });
        UpdateAppearance();
    }

    private void OnInteract(AvatarEvent avatar) {
        if (Opened || ItemKey == null || ItemKey.Length == 0) {
            Opened = true;
            StartCoroutine(Textbox.GetInstance().ShowSystemText("Empty."));
        } else {
            Opened = true;
            UpdateAppearance();
            //ItemData item = ItemData.ItemByName(ItemKey);
            //tartCoroutine(Textbox.GetInstance().ShowSystemText("Found " + item.Name + "."));
        }
    }

    private void UpdateAppearance() {
        string spriteName;
        if (Opened) {
            spriteName = OpenSpriteName;
        } else {
            spriteName = ClosedSpriteName;
        }
        GetComponent<EventAppearance>().SetAppearance(spriteName);
    }

    private string GetSwitchName() {
        MapEvent mapEvent = GetComponent<MapEvent>();
        return mapEvent.Parent.FullName + ":" + mapEvent.Position;
    }
}
