using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Category", menuName = "Game/ItemCategory")]
public class ItemCategory : ScriptableObject {

    public string Name;
    public Sprite Icon;

    public static ItemCategory CategoryByName(string name) {
        return Resources.Load<ItemCategory>("Database/ItemCategories/" + name);
    }

    public static List<ItemCategory> AllCategories() {
        List<ItemCategory> allCategories = new List<ItemCategory>();
        allCategories.Add(CategoryByName("CategoryConsumeable"));
        allCategories.Add(CategoryByName("CategoryDebug"));
        return allCategories;
    }

    public override bool Equals(System.Object other) {
        return Name.Equals(((ItemCategory)other).Name);
    }

    public override int GetHashCode() {
        return Name.GetHashCode();
    }
}
