using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/RPG/Skill")]
public class SkillData : ScriptableObject {

    public string skillName;
    public Sprite skillIcon;
    public SpellSchool school;
    public int baseCost;
    public int basePages;
    public bool prohibitedToBeCD;

    public LuaAnimation castAnimation;

    public Targeter targeter;
    public Effector effect;
}
