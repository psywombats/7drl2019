using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Sprite representations of BattleUnits that exist on the field.
 */
[RequireComponent(typeof(CharaEvent))]
[DisallowMultipleComponent]
public class BattleEvent : MonoBehaviour {

    [HideInInspector]
    public Unit unitData;
    public BattleUnit unit { get; private set; }
    public BattleController controller { get; private set; }

    public void Setup(BattleController controller, BattleUnit unit) {
        this.unit = unit;
        this.controller = controller;
        SetScreenPositionToMatchTilePosition();
    }

    public void PopulateWithUnitData(Unit unitData) {
        this.unitData = unitData;
        if (unitData != null) {
            GetComponent<CharaEvent>().SetAppearance(unitData.appearance.name);
            gameObject.name = unitData.unitName;
        }
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
