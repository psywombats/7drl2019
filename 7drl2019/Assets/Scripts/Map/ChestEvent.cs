using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MapEvent3D))]
public class ChestEvent : MonoBehaviour {

    public SimpleSpriteAnimator doll;
    public Item contents;
    public int quantity;

    public bool opened { get; private set; }

    public void PopulateAndPlace(Item item, Vector2Int loc) {
        contents = item;
        quantity = 1;
        if (item.isGold) {
            quantity = 10 + Random.Range(0, 2 * GetComponent<MapEvent>().map.GetComponent<MapGenerator>().level);
        }
        GetComponent<MapEvent>().SetLocation(loc);
    }

    public IEnumerator OpenRoutine(PCEvent pc) {
        while (pc.GetComponent<MapEvent>().tracking) {
            yield return null;
        }
        pc.GetComponent<BattleEvent>().unit.battle.Log(pc.unit + " found a chest...", true);
        yield return CoUtils.RunSequence(new IEnumerator[] {
            pc.GetComponent<BattleEvent>().AnimateBumpRoutine(),
            doll.PlayOnceRoutine(),
            OnOpenRoutine(pc),
        });
    }

    public IEnumerator OnOpenRoutine(PCEvent pc) {
        if (!pc.GetComponent<BattleEvent>().unit.IsDead()) {
            string qty1 = (quantity > 1) ? "" : "a ";
            string qty2 = (quantity > 1) ? (" x" + quantity) : "";
            pc.GetComponent<BattleEvent>().unit.battle.Log("It contained " + qty1 + contents.ItemName() + qty2 + "!", false);
            pc.PickUpItem(contents, quantity);
        }
        pc.GetComponent<CharaEvent>().facing = EightDir.S;
        pc.GetComponent<CharaEvent>().armMode = ArmMode.Overhead;
        SpriteRenderer sprite = pc.pickup;
        sprite.sprite = contents.sprite;
        yield return CoUtils.Wait(0.8f);
        sprite.sprite = null;
        pc.GetComponent<CharaEvent>().armMode = ArmMode.Disabled;

        opened = true;
        yield return null;
    }
}
