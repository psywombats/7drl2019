using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PartyInventory {

    private List<InventoryEntry> items;

    private Dictionary<ItemData, InventoryEntry> itemsLookup;

    public PartyInventory() {
        items = new List<InventoryEntry>();
        itemsLookup = new Dictionary<ItemData, InventoryEntry>();
    }

    public void AddItem(ItemData item) {
        if (itemsLookup.ContainsKey(item)) {
            itemsLookup[item].Quantity += 1;
        } else {
            InventoryEntry entry = new InventoryEntry(item, 1);
            items.Add(entry);
            itemsLookup[item] = entry;
        }
    }

    public void DeductItem(ItemData item) {
        Assert.IsTrue(itemsLookup.ContainsKey(item));
        InventoryEntry entry = itemsLookup[item];
        entry.Quantity -= 1;
        if (entry.Quantity == 0) {
            itemsLookup.Remove(item);
            items.Remove(entry);
        }
    }

    public bool HasItem(ItemData item) {
        return itemsLookup.ContainsKey(item);
    }

    public InventoryEntry ItemAtIndex(int index) {
        return items[index];
    }

    public int ItemCount() {
        return items.Count;
    }
}