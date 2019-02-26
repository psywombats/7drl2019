using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Unit")]
public class Unit : ScriptableObject {

    public string unitName;
    public Texture2D appearance;
    public bool unique = true;
    public Alignment align;
    

    // tempish
    public Item equippedItem;

    // last for shitty reasons
    public StatSet stats;
}
