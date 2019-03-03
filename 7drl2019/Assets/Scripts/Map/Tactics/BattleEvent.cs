﻿using System.Collections;
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
    public BattleUnit unit { get; set; }
    public BattleController controller { get; private set; }

    private TacticsTerrainMesh _terrain;
    public TacticsTerrainMesh terrain {
        get {
            if (_terrain == null) _terrain = GetComponent<MapEvent>().parent.terrain;
            return _terrain;
        }
    }

    public Vector2Int location { get { return GetComponent<MapEvent3D>().location; } }

    public void Setup(BattleController controller, BattleUnit unit) {
        this.unit = unit;
        this.controller = controller;
        SetScreenPositionToMatchTilePosition();
    }

    public void PopulateWithUnitData(Unit unitData) {
        this.unitData = unitData;
        if (unitData != null) {
            GetComponent<CharaEvent>().spritesheet = unitData.appearance;
            gameObject.name = unitData.unitName;
        }
    }

    public void SetScreenPositionToMatchTilePosition() {
        GetComponent<MapEvent>().SetLocation(unit.location);
    }

    public bool CanCrossTileGradient(Vector2Int from, Vector2Int to) {
        float fromHeight = terrain.HeightAt(from);
        float toHeight = GetComponent<MapEvent>().parent.terrain.HeightAt(to);
        if (toHeight == 0.0f) {
            return false;
        }
        if (fromHeight < toHeight) {
            if (toHeight - fromHeight > unit.GetMaxAscent()) {
                return false;
            }
        } else {
            if (fromHeight - toHeight > unit.GetMaxDescent()) {
                return false;
            }
        }
        if ((from - to).sqrMagnitude > 1.0f) {
            return CanCrossTileGradient(from, new Vector2Int(from.x, to.y)) &&
                CanCrossTileGradient(from, new Vector2Int(to.x, from.y));
        } else {
            return true;
        }
    }

    public bool CanSeeLocation(TacticsTerrainMesh mesh, Vector2Int location) {
        return MathHelper3D.InLos(mesh, this.location, location, unit.Get(StatTag.SIGHT));

    }

    public IEnumerator PostActionRoutine() {
        yield return GetComponent<CharaEvent>().DesaturateRoutine(1.0f);
    }

    public IEnumerator PostTurnRoutine() {
        yield return GetComponent<CharaEvent>().DesaturateRoutine(0.0f);
    }
}
