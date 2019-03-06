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
    [Tooltip("0-100, the higher the more common")]
    [Range(0, 100)]
    public float rarity;
    public LuaAnimation castAnimation;

    [Space]
    public Targeter targeter;
    [Space]
    public Effector effect;

    [Space]
    [TextArea(3, 6)]
    public string description;
}
