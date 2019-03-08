using UnityEngine;

// hacky shit ahead, 7drl, sorry
[CreateAssetMenu(fileName = "Item", menuName = "Data/RPG/Item")]
public class Item : AutoExpandingScriptableObject {

    public string internalName;
    public Sprite sprite;
    public bool isGold;
    public bool isEraser;
    public bool isScissors;

    public virtual string ItemName() {
        return internalName;
    }
}
