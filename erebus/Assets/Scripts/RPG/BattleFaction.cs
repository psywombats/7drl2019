using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * A faction for one alignment or another. Only relevant while currently in battle. Trying to keep
 * this purely check-based and out of coroutines.
 */
public class BattleFaction {

    public Alignment align { get; private set; }
    public Battle battle { get; private set; }

    public BattleFaction(Battle battle, Alignment align) {
        this.align = align;
        this.battle = battle;
    }

    public IEnumerable<BattleUnit> GetUnits() {
        return battle.UnitsByAlignment(align);
    }

    // check if win conditions are met, can vary battle to battle?
    public bool HasWon() {
        // no explicit win conditions by default, but if it's a "escape" game or something, this
        // could go here
        return false;
    }

    // check if loss conditions are met, can vary battle to battle?
    public bool HasLost() {
        return false;
        //// just a deadness check for now
        //foreach (BattleUnit unit in GetUnits()) {
        //    if (unit.IsDead()) {
        //        return false;
        //    }
        //}
        //return true;
    }

    // check if any units in this faction have yet to act in the current turn context
    public bool HasUnitsLeftToAct() {
        return NextMoveableUnit() != null;
    }

    // on start of a new turn, need to set some state on all units
    public IEnumerator OnNewTurnRoutine() {
        foreach (BattleUnit unit in GetUnits()) {
            yield return unit.OnTurnRoutine();
        }
    }

    // returns an available unit that hasn't taken their turn yet
    public BattleUnit NextMoveableUnit() {
        foreach (BattleUnit unit in GetUnits()) {
            if (!unit.hasActedThisTurn) {
                return unit;
            }
        }
        return null;
    }

    public override string ToString() {
        int unitCount = 0;
        foreach (BattleUnit unit in GetUnits()) {
            unitCount += 1;
        }
        return align.AlignmentName() + ": aliveUnits:" + unitCount;
    }
}
