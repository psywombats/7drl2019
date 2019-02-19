using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Unit")]
public class Unit : ScriptableObject {

    public string unitName;
    public AnimatorOverrideController appearance;
    public bool unique = true;
    public Alignment align;
    public StatSet stats;

}
