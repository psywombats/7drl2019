using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scroll", menuName = "Data/RPG/Scroll")]
public class Scroll : Item {

    public SkillData data;
    public List<SkillModifier.Type> mods;

    private Skill _skill;
    public Skill skill {
        get {
            if (_skill == null) _skill = new Skill(this);
            return _skill;
        }
    }

    public override string ItemName() {
        return skill.longformName;
    }
}
