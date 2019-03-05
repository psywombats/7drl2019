using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill {

    private SkillData data;
    private List<SkillModifier> mods;

    public int costMP { get; private set; }
    public int costCD { get; private set; }
    public int pageCost { get; private set; }
    public string longformName { get; private set; }

    public string skillName { get { return data.skillName; } }
    public Sprite icon { get { return data.icon; } }

    public Skill(Scroll scroll) {
        data = scroll.data;
        mods = new List<SkillModifier>();
        foreach (SkillModifier.Type type in scroll.mods) {
            mods.Add(new SkillModifier(type));
        }

        // generation
        if (RandUtils.Chance(0.7f) || data.prohibitedToBeCD) {
            costMP = data.baseCost;
        } else {
            costCD = data.baseCost * 3 / 2;
        }
        pageCost = data.basePages;
        longformName = skillName + "[ " + pageCost + "pg " + (costMP > 0 ? "mp" : "cd") + " ]";
        
        foreach (SkillModifier mod in mods) {
            longformName = mod.MutateName(longformName);
            pageCost = mod.MutateCost(pageCost);
            if (costMP > 0) {
                costMP = mod.MutateCost(costMP);
            } else {
                costCD = mod.MutateCost(costCD);
            }
        }
    }

    public IEnumerator PlaySkillRoutine(BattleUnit actor, Result<IEnumerator> executeResult) {
        Targeter targeter = Object.Instantiate(data.targeter);
        Effector effect = Object.Instantiate(data.effect);
        effect.actor = actor;
        targeter.actor = actor;
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
}
