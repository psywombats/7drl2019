﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Facebox : MonoBehaviour {

    public Image face;
    public NumericalBar hpBar;
    public NumericalBar mpBar;

    private BattleUnit unit;

    public void Populate(BattleUnit unit) {
        this.unit = unit;
        face.sprite = unit.unit.face;
        hpBar.Populate(unit.Get(StatTag.MHP), unit.Get(StatTag.HP));
        mpBar.Populate(unit.Get(StatTag.MP), unit.Get(StatTag.MP));
    }

    public IEnumerator OnTurnAction() {
        if (unit != null && !unit.IsDead()) {
            return CoUtils.RunParallel(new IEnumerator[] {
                hpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MHP), unit.Get(StatTag.HP), 0.125f),
                mpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MMP), unit.Get(StatTag.MP), 0.125f),
            }, this);
        } else {
            return CoUtils.Wait(0.0f);
        }
    }
}