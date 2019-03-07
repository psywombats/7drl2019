using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Facebox : MonoBehaviour {

    public Image face;
    public NumericalBar hpBar;
    public NumericalBar mpBar;
    public NumericalBar cdBar;
    
    private BattleUnit unit;

    public void Populate(BattleUnit unit) {
        this.unit = unit;
        face.sprite = unit.unit.face;
        hpBar.Populate(unit.Get(StatTag.MHP), unit.Get(StatTag.HP));
        mpBar.Populate(unit.Get(StatTag.MMP), unit.Get(StatTag.MP));
        cdBar.Populate(unit.maxCD, unit.Get(StatTag.CD));
    }

    public void OnTurn() {
        if (unit != null && !unit.IsDead()) {
            StartCoroutine(CoUtils.RunParallel(new IEnumerator[] {
                hpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MHP), unit.Get(StatTag.HP), 0.125f),
                mpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MMP), unit.Get(StatTag.MP), 0.125f),
                cdBar.AnimateWithTimeRoutine(unit.maxCD, unit.Get(StatTag.CD), 0.125f),
            }, this));
        }
    }

    public void SetFaceNumber(int faceNo) {
        if (faceNo == 0) {
            face.sprite = unit.unit.face;
        } else {
            face.sprite = unit.unit.altFaces[faceNo];
        }
    }
}
