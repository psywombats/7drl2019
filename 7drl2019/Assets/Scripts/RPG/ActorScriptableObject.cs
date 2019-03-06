using UnityEngine;

// common ancestor of effectors, targeters so that they share some easy actor accessors
public abstract class ActorScriptableObject : AutoExpandingScriptableObject {

    public Skill skill { get; set; }
    public BattleUnit actor { get; set; }
    protected BattleController battle { get { return actor.battle; } }
    protected BattleEvent battler { get { return actor.battler; } }
    protected MapEvent actorEvent { get { return battler.GetComponent<MapEvent>(); } }
    protected Map map { get { return battle.map; } }
    protected TacticsTerrainMesh terrain { get { return map.terrain; } }

}
