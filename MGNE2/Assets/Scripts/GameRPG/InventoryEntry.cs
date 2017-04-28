using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryEntry {

    public ItemData Item;
    public int Quantity;

    public InventoryEntry(ItemData item, int quantity) {
        this.Item = item;
        this.Quantity = quantity;
    }
}
