using System.Collections;
using UnityEngine;

// representation of a unit in battle
public class BattleUnit {

    public Unit unit { get; private set; }
    public Battle battle { get; private set; }
    public BattleController controller { get { return battle.controller; } }
    public Alignment align { get { return unit.align; } }
    public Vector2Int location { get; set; }
    public bool hasActedThisTurn { get; private set; }

    public BattleEvent battler {
        get {
            return battle.controller.GetDollForUnit(this);
        }
    }

    // === INITIALIZATION ==========================================================================

    // we create battle units from three sources
    //  - unit, this is a keyed by what comes in from tiled and used to look up hero/enemy in db
    //  - battle, the parent battle creating this unit for
    //  - starting location, gleened from the tiled event usually
    public BattleUnit(Unit unit, Battle battle, Vector2Int location) {
        this.unit = unit;
        this.battle = battle;
        this.location = location;
    }

    // === RPG =====================================================================================

    public float Get(StatTag tag) {
        return unit.stats.Get(tag);
    }

    public bool Is(StatTag tag) {
        return unit.stats.Is(tag);
    }

    // checks for deadness and dead-like conditions like petrification
    public bool IsDead() {
        return unit.stats.Get(StatTag.HP) <= 0;
    }

    public int GetMaxAscent() {
        return (int)unit.stats.Get(StatTag.JUMP);
    }

    public int GetMaxDescent() {
        return (int)unit.stats.Get(StatTag.JUMP) + 1;
    }

    // === STATE MACHINE ===========================================================================

    // called at the beginning of this unit's faction's turn
    public IEnumerator OnTurnRoutine() {
        yield return null;
        hasActedThisTurn = false;
    }

    // actually do the menu
    public IEnumerator SelectSkillRoutine(Result<Skill> result) {
        // TODO: ui
        result.value = unit.knownSkills[0];
        yield return null;
    }

    // select + execute a skill
    public IEnumerator PlayNextActionRoutine(IEnumerator cancelRoutine) {
        Result<Skill> skillResult = new Result<Skill>();
        yield return SelectSkillRoutine(skillResult);
        if (skillResult.canceled) {
            yield return cancelRoutine;
        } else {
            Result<Effector> effectResult = new Result<Effector>();
            yield return skillResult.value.PlaySkillRoutine(this, effectResult);
            if (effectResult.canceled) {
                yield return PlayNextActionRoutine(cancelRoutine);
            } else {
                // TODO: add the effect to the undo stack
                yield return PostActionRoutine();
            }
        }
    }

    // called at the end of this unit's action
    private IEnumerator PostActionRoutine() {
        yield return battler.PostActionRoutine();
        hasActedThisTurn = true;
    }
}
