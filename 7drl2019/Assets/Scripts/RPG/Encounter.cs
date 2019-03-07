using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Encounter")]
public class Encounter : AutoExpandingScriptableObject {

    public int danger = 375;
    [Range(0, 100)]
    public float rarity = 50;
    public List<Unit> units;
    
}
