using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Unit")]
public class Unit : ScriptableObject {

    public string unitName;
    public bool unique;
    public StatSet stats;

}
