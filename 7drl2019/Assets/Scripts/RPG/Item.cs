using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Data/RPG/Item")]
public class Item : AutoExpandingScriptableObject {

    public string internalName;
    public Sprite sprite;

    public virtual string ItemName() {
        return internalName;
    }
}
