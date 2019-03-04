using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class SkillsetUI : MonoBehaviour {

    public SkillContainer containerPrefab;

    private BattleUnit unit;

    public void Populate(BattleUnit unit) {
        Clear();
        for (int i = 0; i < unit.unit.knownSkills.Count; i += 1) {
            SkillContainer container = Instantiate(containerPrefab);
            container.transform.SetParent(transform);
            container.Populate(unit, i);
        }
    }

    public IEnumerator OnTurnAction() {
        List<IEnumerator> toRun = new List<IEnumerator>();
        foreach (Transform child in transform) {
            toRun.Add(child.GetComponent<SkillContainer>().UpdateAction());
        }
        return CoUtils.RunParallel(toRun.ToArray(), this);
    }

    private void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
