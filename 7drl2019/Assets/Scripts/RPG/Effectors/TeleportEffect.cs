using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportEffect : Effector {

    public LuaAnimation fadeoutAnim;
    public LuaAnimation fadeinAnim;

    public override bool TargetsHostiles() {
        return false;
    }

    public override bool AcceptsEmptyGrids() {
        return true;
    }

    public override bool AcceptsFullGrids() {
        return false;
    }

    public override IEnumerator ExecuteCellsRoutine(List<Vector2Int> locations) {
        yield return battler.SyncPlayAnim(fadeoutAnim);
        actorEvent.SetLocation(locations[Random.Range(0, locations.Count)]);
        yield return battler.SyncPlayAnim(fadeinAnim);
    }
}
