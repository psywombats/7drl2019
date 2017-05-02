using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCategoryCell : MonoBehaviour {

    public Image CategoryIcon;
    public Image Cursor;

    public void Populate(ItemCategory category, bool selected) {
        CategoryIcon.sprite = category.Icon;
        Cursor.enabled = selected;
    }

}
