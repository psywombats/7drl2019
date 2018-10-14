using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[ExecuteInEditMode]
[RequireComponent(typeof(CharaEvent))]
public class Doll : MonoBehaviour {

    private static string InstancePath = "Prefabs/Map3D/Doll";

    // Editor properties
    public SpriteRenderer Sprite;
    public bool BillboardedX = true;

    public BattleUnit Unit { get; private set; }
    public BattleController Controller { get; private set; }

    public static Doll Instantiate(BattleController controller, BattleUnit unit) {
        Doll instance = Instantiate(Resources.Load<GameObject>(InstancePath)).GetComponent<Doll>();
        instance.Setup(controller, unit);
        return instance;
    }

    public void Setup(BattleController controller, BattleUnit unit) {
        this.Unit = unit;
        this.Controller = controller;
        SetScreenPositionToMatchTilePosition();
    }

    public void Update() {
        if (BillboardedX) {
            Vector3 angles = Sprite.transform.eulerAngles;
            Sprite.transform.eulerAngles = new Vector3(
                    TacticsCam.Instance().transform.eulerAngles.x, 
                    angles.y, 
                    angles.z);
        }
    }
    
    public void SetScreenPositionToMatchTilePosition() {
        GetComponent<MapEvent>().SetLocation(Unit.Location);
    }
}
