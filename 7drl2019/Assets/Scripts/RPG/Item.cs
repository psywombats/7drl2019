using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Data/RPG/Item")]
public class Item : ScriptableObject {

    public string internalName;
    public Sprite sprite;

    public string ItemName() {
        return internalName;
    }
}
