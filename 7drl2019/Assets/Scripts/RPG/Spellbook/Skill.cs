using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill {

    public SkillData data { get; private set; }
    public Scroll scroll { get; private set; }
    public List<SkillModifier> mods { get; private set; }

    public int costMP { get; private set; }
    public int costCD { get; private set; }
    public int pageCost { get; private set; }
    public string longformName { get; private set; }

    public string skillName { get { return data.skillName; } }
    public Sprite icon { get { return data.skillIcon; } }
    public SpellSchool school { get { return data.school; } }
    public LuaAnimation castAnim { get { return data.castAnimation; } }

    public Skill(SkillData data) {
        this.data = data;
        costMP = data.baseCost;
        // 7drl hack
    }

    public Skill(Scroll scroll) {
        this.scroll = scroll;
        data = scroll.data;
        mods = new List<SkillModifier>();
        foreach (SkillModifier.Type type in scroll.mods) {
            mods.Add(new SkillModifier(type));
        }

        // generation
        if (data.prohibitedToBeCD) {
            costMP = data.baseCost;
        } else if (data.prohibitedToBeMP) {
            costCD = data.baseCost;
        } else if (RandUtils.Chance(0.7f)) {
            costMP = data.baseCost;
        } else {
            costCD = Mathf.CeilToInt(data.baseCost / 8.0f);
        }

        pageCost = data.basePages;
        longformName = "";
        foreach (SkillModifier mod in mods) {
            longformName = mod.MutateName(longformName);
            pageCost = mod.MutatePages(pageCost);
            if (costMP > 0) {
                costMP = mod.MutateCost(costMP);
            } else {
                costCD = mod.MutateCost(costCD);
            }
        }
        longformName = skillName + " [ " + pageCost + "pg " + (costMP > 0 ? costMP + "mp" : costCD + "cd") + " ] " + longformName;
    }

    public IEnumerator PlaySkillRoutine(BattleUnit actor, Result<bool> executeResult) {
        actor.battle.Log(actor + " casts " + skillName + "...", true);
        Targeter targeter = Object.Instantiate(data.targeter);
        Effector effect = Object.Instantiate(data.effect);
        effect.actor = actor;
        targeter.actor = actor;
        effect.skill = this;
        targeter.skill = this;
        yield return targeter.ExecuteRoutine(effect, executeResult);
        if (!executeResult.canceled) {
            if (costMP > 0) {
                actor.unit.stats.Sub(StatTag.MP, costMP);
            }
            if (costCD > 0) {
                actor.unit.stats.Add(StatTag.CD, costCD);
                actor.maxCD = costCD;
            }
        }
    }

    public IEnumerator TryAIUse(AIController ai) {
        Targeter targeter = Object.Instantiate(data.targeter);
        Effector effect = Object.Instantiate(data.effect);
        effect.actor = ai.unit;
        targeter.actor = ai.unit;
        effect.skill = this;
        targeter.skill = this;
        return targeter.TryAIUse(ai, effect);
    }
}
