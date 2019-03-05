using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/RPG/Skill")]
public class SkillData : ScriptableObject {

    public string skillName;
    public Sprite icon;
    public int baseCost;
    public int basePages;
    public bool prohibitedToBeCD;

    public Targeter targeter;
    public Effector effect;
}
