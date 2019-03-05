using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MapEvent3D))]
public class ChestEvent : MonoBehaviour {

    public SimpleSpriteAnimator doll;
    public Item contents;
    public int quantity;

    public bool opened { get; private set; }

    public IEnumerator OpenAction(PCEvent pc) {
        pc.GetComponent<BattleEvent>().unit.battle.Log(pc.unit + " found a chest...");
        return CoUtils.RunSequence(new IEnumerator[] {
            pc.GetComponent<BattleEvent>().AnimateBumpAction(),
            doll.PlayOnceRoutine(),
            OnOpenRoutine(pc),
        });
    }

    public IEnumerator OnOpenRoutine(PCEvent pc) {
        if (!pc.GetComponent<BattleEvent>().unit.IsDead()) {
            string qty1 = (quantity > 1) ? "" : "a ";
            string qty2 = (quantity > 1) ? (" x" + quantity) : "";
            pc.GetComponent<BattleEvent>().unit.battle.Log("It contained " + qty1 + contents.ItemName() + qty2 + "!", true);
            pc.PickUpItem(contents, quantity);
        }
        opened = true;
        yield return null;
    }
}
