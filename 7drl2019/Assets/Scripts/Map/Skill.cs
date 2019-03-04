using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/RPG/Skill")]
public class Skill : ScriptableObject {

    public string skillName;
    public int mpCost;
    public Sprite icon;

    public Targeter targeter;
    public Effector effect;

    public IEnumerator PlaySkillRoutine(BattleUnit actor, Result<Effector> effectResult) {
        Targeter targeter = Instantiate(this.targeter);
        Effector effect = Instantiate(this.effect);
        effect.actor = actor;
        targeter.actor = actor;
        yield return targeter.ExecuteRoutine(effect, effectResult);
    }
}
