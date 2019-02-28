using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/RPG/Skill")]
public class Skill : ScriptableObject {

    public string skillName;
    public int apCost;

    public Targeter.TargeterParams targeter;
    public Effector.EffectorParams effect;
}
