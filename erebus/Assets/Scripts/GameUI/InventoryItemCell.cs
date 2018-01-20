using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCell : MonoBehaviour {

    public Image Icon;
    public Text ItemName;
    public Text ItemQuantity;
    public Image Cursor;

    public void Populate(InventoryEntry entry, bool selected) {
        Icon.sprite = entry.Item.IconSmall;
        ItemName.text = entry.Item.Name;
        ItemQuantity.text = "x" + entry.Quantity;
        Cursor.gameObject.SetActive(selected);
    }
}
