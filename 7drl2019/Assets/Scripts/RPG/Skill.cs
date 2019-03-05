using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Data/RPG/Skill")]
public class Skill : ScriptableObject {

    public string skillName;
    public int costMP;
    public int costCD;
    public Sprite icon;

    public Targeter targeter;
    public Effector effect;

    public IEnumerator PlaySkillRoutine(BattleUnit actor, Result<IEnumerator> executeResult) {
        Targeter targeter = Instantiate(this.targeter);
        Effector effect = Instantiate(this.effect);
        effect.actor = actor;
        targeter.actor = actor;
        yield return targeter.ExecuteRoutine(effect, executeResult);
        if (!executeResult.canceled) {
            if (costMP > 0) {
                actor.unit.stats.Sub(StatTag.MP, costMP);
            }
            if (costCD > 0) {
                actor.unit.stats.Add(StatTag.CD, costCD);
                actor.maxCD = costCD;
            }
        }
    }
}
