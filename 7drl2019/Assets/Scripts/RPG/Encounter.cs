using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Unit", menuName = "Data/RPG/Encounter")]
public class Encounter : AutoExpandingScriptableObject {

    private const string PrefabPath = "Prefabs/Enemy";

    public int levelMin;
    public int levelMax;
    [Range(0, 100)]
    public float rarity = 50;
    public List<Unit> units;
    
    public void PlaceAt(Map map, Vector2Int loc) {
        
        Vector2Int toLoc = loc;
        BattleEvent leader = null;
        foreach (Unit unit in units) {
            GameObject enemyObject = Instantiate(Resources.Load<GameObject>(PrefabPath));
            map.AddEvent(enemyObject.GetComponent<MapEvent>());
            enemyObject.GetComponent<BattleEvent>().PopulateWithUnitData(unit);
            map.GetComponent<BattleController>().AddUnitFromMap(enemyObject.GetComponent<BattleEvent>());
            if (leader == null) {
                leader = enemyObject.GetComponent<BattleEvent>();
            }
            foreach (EightDir dir in EightDirExtensions.RandomOrder()) {
                toLoc = loc + dir.XY();
                if (enemyObject.GetComponent<MapEvent>().CanPassAt(toLoc)) {
                    break;
                }
            }
            if (toLoc.x == map.size.x - 1) {
                toLoc.x -= 1;
            }
            if (toLoc.y == map.size.y - 1) {
                toLoc.y -= 1;
            }
            enemyObject.GetComponent<MapEvent>().SetLocation(toLoc);
            if (leader != null) {
                enemyObject.GetComponent<BattleEvent>().unit.ai.leader = leader;
                enemyObject.GetComponent<BattleEvent>().unit.ai.leader = leader;
            }
        }
    }
}
