using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AIController {

    private const float PathingCutoffInt = 5;
    private const float WanderCutoffInt = 5;

    public BattleUnit unit { get; private set; }
    public BattleEvent battler { get { return unit.battler; } }
    public BattleUnit pc { get { return unit.battle.pc; } }
    public BattleController battle { get { return unit.battle; } }

    private short[,] seenMap;
    private int turnsHunting;

    public AIController(BattleUnit unit) {
        this.unit = unit;
        seenMap = new short[battle.map.size.x, battle.map.size.y];
    }

    public IEnumerator TakeTurnRoutine() {
        int intel = (int)unit.Get(StatTag.INTELLIGENCE);

        Result<bool> result = new Result<bool>();

        seenMap[battler.location.x, battler.location.y] += 1;
        turnsHunting -= 1;

        // hunt down the hero if we've recently seen them
        if (battler.CanSeeLocation(battle.map.terrain, pc.location)) {
            turnsHunting = intel;
        }
        if (turnsHunting > 0) {
            if (intel >= PathingCutoffInt) {
                List<Vector2Int> path = battle.map.FindPath(battler.GetComponent<MapEvent>(), pc.location, intel);
                if (path != null && path.Count > 0) {
                    return battler.StepOrAttackRoutine(battler.GetComponent<MapEvent>().DirectionTo(path[0]), result);
                }
            } else {
                return battler.StepOrAttackRoutine(battler.GetComponent<MapEvent>().DirectionTo(pc.location), result);
            }
        }

        // wander randomly
        EightDir bestDir = EightDirExtensions.RandomDir();
        if (intel > WanderCutoffInt) {
            short lowestSeen = short.MaxValue;
            foreach (EightDir dir in EightDirExtensions.RandomOrder()) {
                Vector2Int target = battler.location + dir.XY();
                if (target.x < 0 || target.x >= battle.map.size.x || target.y < 0 || target.y >= battle.map.size.y) {
                    continue;
                }
                if (seenMap[target.x, target.y] < lowestSeen && battler.GetComponent<MapEvent>().CanPassAt(target)) {
                    lowestSeen = seenMap[target.x, target.y];
                    bestDir = dir;
                }
            }
        }
        return battler.StepOrAttackRoutine(bestDir, result);
    }
}
