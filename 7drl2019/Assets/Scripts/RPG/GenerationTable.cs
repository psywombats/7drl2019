using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GenerationTable", menuName = "Data/RPG/Generation Table")]
public class GenerationTable : ScriptableObject {

    public List<Encounter> encounters;

    public int baseDanger;
    public int dangerPerLevel;
    public List<int> firstLevelDangerOverrides;

    public List<Encounter> GenerateEncounters(int level) {
        int targetDanger;
        if (level < firstLevelDangerOverrides.Count) {
            targetDanger = firstLevelDangerOverrides[level];
        } else {
            targetDanger = baseDanger + level * dangerPerLevel;
        }

        List<Encounter> results = new List<Encounter>();
        int currentDanger = 0;
        while (currentDanger < targetDanger) {
            RandUtils.Shuffle(encounters);
            Encounter toAdd = null;
            foreach (Encounter encounter in encounters) {
                if (encounter.danger < targetDanger / 3.5f 
                        && encounter.danger > targetDanger / 20.0f
                        && encounter.danger + currentDanger <= targetDanger * 1.1f
                        && RandUtils.Chance(encounter.rarity / 100.0f)) {
                    toAdd = encounter;
                    currentDanger += encounter.danger;
                    break;
                }
            }
            if (toAdd == null) {
                currentDanger += 50;
            } else {
                results.Add(toAdd);
            }
        }
        return results;
    }
}
