using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCell : MonoBehaviour {

    public Image Icon;
    public Text ItemName;
    public Text ItemQuantity;

    public void Populate(InventoryEntry entry) {
        Icon.sprite = entry.Item.IconSmall;
        ItemName.text = entry.Item.name;
        ItemQuantity.text = "x" + entry.Quantity;
    }
}
