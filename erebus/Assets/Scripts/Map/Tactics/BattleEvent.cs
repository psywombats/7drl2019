using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[ExecuteInEditMode]
[RequireComponent(typeof(CharaEvent))]
public class BattleEvent : TiledInstantiated {

    private static string InstancePath = "Prefabs/Map3D/Doll";

    // Editor properties
    public SpriteRenderer sprite {
        get {
            if (GetComponent<CharaEvent>().doll == null) {
                return null;
            } else {
                return GetComponent<CharaEvent>().doll.GetComponent<SpriteRenderer>();
            }
        }
    }
    public bool billboardX = true;
    public BattleUnit unit { get; private set; }
    public BattleController controller { get; private set; }

    public static BattleEvent Instantiate(BattleController controller, BattleUnit unit) {
        BattleEvent instance = Instantiate(Resources.Load<GameObject>(InstancePath)).GetComponent<BattleEvent>();
        instance.Setup(controller, unit);
        return instance;
    }

    public void Setup(BattleController controller, BattleUnit unit) {
        this.unit = unit;
        this.controller = controller;
        SetScreenPositionToMatchTilePosition();
    }

    public override void Populate(IDictionary<string, string> properties) {
        string unitKey = properties[MapEvent.PropertyUnit];
        GetComponent<MapEvent3D>().Parent.battleController.AddUnitFromTiledEvent(this, unitKey);
    }

    public void Update() {
        if (billboardX && sprite != null) {
            Vector3 angles = sprite.transform.eulerAngles;
            sprite.transform.eulerAngles = new Vector3(
                    TacticsCam.Instance().transform.eulerAngles.x, 
                    angles.y, 
                    angles.z);
        }
    }

    public void SetScreenPositionToMatchTilePosition() {
        GetComponent<MapEvent>().SetLocation(unit.location);
    }
}
