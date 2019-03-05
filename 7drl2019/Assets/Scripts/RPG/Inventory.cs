using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory  {

    private Dictionary<Item, int> contents;

    public Inventory() {
        contents = new Dictionary<Item, int>();
    }

    public void Add(Item item, int count = 1) {
        if (contents.ContainsKey(item)) {
            contents[item] += count;
        } else {
            contents[item] = count;
        }
    }

    public int Count(Item item) {
        if (contents.ContainsKey(item)) {
            return contents[item];
        } else {
            return 0;
        }
    }

    public void Remove(Item item) {
        Debug.Assert(Has(item));
        contents[item] -= 1;
    }

    public bool Has(Item item) {
        return Count(item) > 0;
    }
}
