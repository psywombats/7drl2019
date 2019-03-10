using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Unit")]
public class Unit : ScriptableObject {

    public string unitName;
    public Texture2D appearance;
    public Sprite face;
    public List<Sprite> altFaces;
    public bool unique = true;
    public Alignment align;

    [Tooltip("Monsters will use this")]
    public List<SkillData> innateSkills;

    [TextArea(3, 6)] public string luaOnExamine;
    [TextArea(3, 6)] public string luaOnDefeat;
    public List<string> flightMessages;

    // tempish
    public Item equippedItem;

    // last for shitty reasons
    public StatSet stats;
}
