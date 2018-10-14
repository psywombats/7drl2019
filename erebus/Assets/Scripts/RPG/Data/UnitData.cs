using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "UnitData", menuName = "Data/RPG")]
public class UnitData : ScriptableObject {

    public String Name;

    [Space(20)]
    public AdditiveStat MHP;
    public AdditiveStat HP;

    [Space(20)]
    public Alignment Align;
}
