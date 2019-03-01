using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * A battle in progress. Responsible for all battle logic, state, and control flow. The actual
 * battle visual representation is contained in the BattleController. 
 * 
 * Flow for battles works like this:
 *  - A BattleController exists on a 3d map
 *  - The BattleController holds an instance of this class
 *  - At the start of the battle, we pick up all units with a battle event
 */
 [System.Serializable]
public class Battle {

    public AIController ai;
    
    public BattleController controller { get; private set; }
    
    private List<BattleUnit> units;
    private Dictionary<Alignment, BattleFaction> factions;

    // === INITIALIZATION ==========================================================================

    public Battle() {
        units = new List<BattleUnit>();
        factions = new Dictionary<Alignment, BattleFaction>();
    }

    // === BOOKKEEPING AND GETTERS =================================================================

    public ICollection<BattleUnit> AllUnits() {
        return units;
    }

    public IEnumerable<BattleUnit> UnitsByAlignment(Alignment align) {
        return units.Where(unit => (unit.align == align));
    }

    // if we see someone with Malu's unit, we should add the Malu instance, eg
    public BattleUnit AddUnitFromSerializedUnit(Unit unit, Vector2Int startingLocation) { 
        Unit instance = Global.Instance().Party.LookUpUnit(unit.name);
        BattleUnit battleUnit = new BattleUnit(unit, this, startingLocation);

        AddUnit(battleUnit);

        if (!factions.ContainsKey(battleUnit.align)) {
            factions[battleUnit.align] = new BattleFaction(this, battleUnit.align);
        }

        return battleUnit;
    }

    public List<BattleFaction> GetFactions() {
        return new List<BattleFaction>(factions.Values);
    }

    public BattleFaction GetFaction(Alignment align) {
        return factions[align];
    }

    private void AddUnit(BattleUnit unit) {
        units.Add(unit);
    }

    // === STATE MACHINE ===========================================================================

    // runs and executes this battle
    public IEnumerator BattleRoutine(BattleController controller) {
        this.controller = controller;
        ai.ConfigureForBattle(this);
        while (true) {
            yield return NextRoundRoutine();
            if (CheckGameOver() != Alignment.None) {
                yield break;
            }
        }
    }
    
    // coroutine to play out a single round of combat
    private IEnumerator NextRoundRoutine() {
        yield return PlayTurnRoutine(Alignment.Hero);
        if (CheckGameOver() != Alignment.None) {
            yield break;
        }
        yield return PlayTurnRoutine(Alignment.Enemy);
        if (CheckGameOver() != Alignment.None) {
            yield break;
        }
    }

    // returns which alignment won the game, or Alignment.None if no one did
    private Alignment CheckGameOver() {
        foreach (BattleFaction faction in factions.Values) {
            if (faction.HasWon()) {
                return faction.align;
            } else if (faction.align == Alignment.Hero && faction.HasLost()) {
                return Alignment.Enemy;
            } else if (faction.align == Alignment.Enemy && faction.HasLost()) {
                return Alignment.Hero;
            }
        }
        return Alignment.None;
    }

    // responsible for changing ui state to this unit's turn, then 
    private IEnumerator PlayTurnRoutine(Alignment align) {
        if (!factions.ContainsKey(align)) {
            yield break;
        }
        yield return factions[align].OnNewTurnRoutine();
        yield return controller.TurnBeginAnimationRoutine(align);
        while (factions[align].HasUnitsLeftToAct()) {
            yield return PlayNextActionRoutine(align);
        }
        yield return controller.TurnEndAnimationRoutine(align);
    }

    private IEnumerator PlayNextActionRoutine(Alignment align) {
        switch (align) {
            case Alignment.Hero:
                yield return PlayNextHumanActionRoutine();
                break;
            case Alignment.Enemy:
                yield return ai.PlayNextAIActionRoutine();
                yield break;
            default:
                Debug.Assert(false, "bad align " + align);
                yield break;
        }
    }

    private IEnumerator PlayNextHumanActionRoutine() {
        controller.MoveCursorToDefaultUnit();

        Result<BattleUnit> unitResult = new Result<BattleUnit>();
        yield return controller.SelectUnitRoutine(unitResult, (BattleUnit unit) => {
            return unit.align == Alignment.Hero && !unit.hasActedThisTurn;
        }, false);
        BattleUnit actingUnit = unitResult.value;
        yield return actingUnit.PlayNextActionRoutine(PlayNextHumanActionRoutine());


        //// TODO: remove this nonsense
        //Result<BattleUnit> targetedResult = new Result<BattleUnit>();
        //yield return controller.SelectAdjacentUnitRoutine(targetedResult, actingUnit, (BattleUnit unit) => {
        //    return unit.align == Alignment.Enemy;
        //});
        //if (targetedResult.canceled) {
        //    // TODO: reset where they came from
        //    yield return PlayNextHumanActionRoutine();
        //    yield break;
        //}
        //BattleUnit targetUnit = targetedResult.value;
        //targetUnit.doll.GetComponent<CharaEvent>().FaceToward(actingUnit.doll.GetComponent<MapEvent>());

        //yield return Global.Instance().Maps.activeDuelMap.EnterMapRoutine(actingUnit.doll, targetUnit.doll);
        //yield return Global.Instance().Maps.activeDuelMap.ExitMapRoutine();

    }
}
