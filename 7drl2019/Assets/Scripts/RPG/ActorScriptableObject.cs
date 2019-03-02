using UnityEngine;

// common ancestor of effectors, targeters so that they share some easy actor accessors
public abstract class ActorScriptableObject : ScriptableObject {

    public BattleUnit actor { get; set; }
    protected Battle battle { get { return actor.battle; } }
    protected BattleController controller { get { return battle.controller; } }
    protected BattleEvent battler { get { return actor.battler; } }
    protected MapEvent mapEvent { get { return battler.GetComponent<MapEvent>(); } }
    protected Map map { get { return controller.map; } }
    protected TacticsTerrainMesh terrain { get { return map.terrain; } }

}
