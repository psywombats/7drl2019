using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class BattleEvent : MonoBehaviour {

    private static readonly string InstancePath = "Prefabs/Map3D/Doll";

    public string unitKey;
    public BattleUnit unit { get; private set; }
    public BattleController controller { get; private set; }

    public static BattleEvent GetInstance(BattleController controller, BattleUnit unit) {
        BattleEvent instance = Instantiate(Resources.Load<GameObject>(InstancePath)).GetComponent<BattleEvent>();
        instance.Setup(controller, unit);
        return instance;
    }

    public void Setup(BattleController controller, BattleUnit unit) {
        this.unit = unit;
        this.controller = controller;
        SetScreenPositionToMatchTilePosition();
    }

    public void OnEnable() {
        BattleController controller = GetComponent<MapEvent3D>().parent.GetComponent<BattleController>();
        controller.AddUnitFromTiledEvent(this, unitKey);
    }

    public void SetScreenPositionToMatchTilePosition() {
        GetComponent<MapEvent>().SetLocation(unit.location);
    }

    public IEnumerator PostActionRoutine() {
        yield return GetComponent<CharaEvent>().animator.DesaturateRoutine(1.0f);
    }

    public IEnumerator PostTurnRoutine() {
        yield return GetComponent<CharaEvent>().animator.DesaturateRoutine(0.0f);
    }
}
