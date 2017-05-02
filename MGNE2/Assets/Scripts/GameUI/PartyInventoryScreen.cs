using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyInventoryScreen : MonoBehaviour, InputListener {

    public GridLayoutGroup InventoryGroup;
    public GridLayoutGroup CategoryGroup;
    public Text ItemDescription;
    public Text CategoryName;
    public Image ImageIcon;

    public int InventoryCellCount;
    public int CategoryCellCount;

    private static PartyInventoryScreen instance;

    private PartyInventory inventory;
    private List<InventoryEntry> categoryItems;
    private List<ItemCategory> categories;
    private ItemCategory activeCategory;

    private int scrollOffset;
    private int itemCursor, categoryCursor;
    private bool transitioning;
    private bool categoryMode;

    public static PartyInventoryScreen GetInstance() {
        if (instance == null) {
            instance = FindObjectOfType<PartyInventoryScreen>();
        }
        return instance;
    }

    public void Populate() {
        inventory = GGlobal.Instance().Party.Inventory;
        categories = ItemCategory.AllCategories();

        scrollOffset = 0;
        categoryCursor = 0;
        transitioning = false;
        ItemDescription.text = "";
        ImageIcon.sprite = null;

        ReloadCategory();
        categoryMode = (categoryItems.Count == 0);
        UpdateDisplay();
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (transitioning) {
            return true;
        }
        if (eventType != InputManager.Event.Down) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Cancel:
                StartCoroutine(TransitionOut());
                break;
            case InputManager.Command.Confirm:
                Select();
                break;
            case InputManager.Command.Down:
                MoveCursor(1);
                break;
            case InputManager.Command.Up:
                MoveCursor(-1);
                break;
            case InputManager.Command.Left:
            case InputManager.Command.Right:
                SwitchMode();
                break;
        }
        return true;
    }

    public void UpdateDisplay() {
        for (int i = 0; i < CategoryCellCount; i += 1) {
            InventoryCategoryCell cell = CategoryGroup.GetComponentsInChildren<InventoryCategoryCell>(true)[i];
            if (i < categories.Count) {
                cell.gameObject.SetActive(true);
                ItemCategory category = categories[i];
                cell.Populate(category, categoryCursor == i && categoryMode);
            } else {
                cell.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < InventoryCellCount; i += 1) {
            int index = scrollOffset + i;
            InventoryItemCell cell = InventoryGroup.GetComponentsInChildren<InventoryItemCell>(true)[i];
            if (index < categoryItems.Count) {
                cell.gameObject.SetActive(true);
                InventoryEntry entry = categoryItems[index];
                cell.Populate(entry, itemCursor == i && !categoryMode);
            } else {
                cell.gameObject.SetActive(false);
            }
        }
        int selectedIndex = scrollOffset + itemCursor;
        if (selectedIndex < inventory.ItemCount()) {
            InventoryEntry selectedEntry = inventory.ItemAtIndex(selectedIndex);
            ItemDescription.text = selectedEntry.Item.Description;
        }
    }

    public IEnumerator TransitionIn() {
        transitioning = true;

        Populate();
        Global.Instance().Input.PushListener(this);

        // placeholder
        while (GetComponent<CanvasGroup>().alpha < 1.0f) {
            GetComponent<CanvasGroup>().alpha += Time.deltaTime / 0.25f;
            if (GetComponent<CanvasGroup>().alpha > 1.0f) {
                GetComponent<CanvasGroup>().alpha = 1.0f;
            }
            yield return null;
        }

        transitioning = false;
    }

    public IEnumerator TransitionOut() {
        transitioning = true;

        // placeholder
        while (GetComponent<CanvasGroup>().alpha > 0.0f) {
            GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 0.25f;
            if (GetComponent<CanvasGroup>().alpha < 0.0f) {
                GetComponent<CanvasGroup>().alpha = 0.0f;
            }
            yield return null;
        }

        Global.Instance().Input.RemoveListener(this);
        transitioning = false;
    }

    private void SwitchMode() {
        if (categoryMode) {
            if (categoryItems.Count > 0) {
                categoryMode = false;
            }
        } else {
            categoryMode = true;
        }
        UpdateDisplay();
    }

    private void ReloadCategory() {
        itemCursor = 0;
        activeCategory = categories[categoryCursor];
        categoryItems = inventory.ItemsByCategory(activeCategory);
        CategoryName.text = activeCategory.Name;
        UpdateEntry();
        UpdateDisplay();
    }

    private void UpdateEntry() {
        if (categoryItems.Count == 0) {
            ImageIcon.enabled = false;
            ItemDescription.text = "";
        } else {
            InventoryEntry entry = categoryItems[itemCursor + scrollOffset];
            ImageIcon.enabled = true;
            ItemDescription.text = entry.Item.Description;
            ImageIcon.sprite = entry.Item.IconBig;
        }
    }

    private void MoveCursor(int delta) {
        if (categoryMode) {
            categoryCursor += delta;
            if (categoryCursor > categories.Count - 1) {
                categoryCursor = 0;
            } else if (categoryCursor < 0) {
                categoryCursor = categories.Count - 1;
            }
            ReloadCategory();
        } else {
            int index = itemCursor + scrollOffset;
            if (index == 0 && delta < 0) {
                itemCursor = Mathf.Min(InventoryCellCount - 1, categoryItems.Count - 1);
                scrollOffset = categoryItems.Count - InventoryCellCount;
                if (scrollOffset < 0) {
                    scrollOffset = 0;
                }
            } else if (index == categoryItems.Count - 1 && delta > 0) {
                scrollOffset = 0;
                itemCursor = 0;
            } else {
                itemCursor += delta;
                if (delta > 0 && (itemCursor == InventoryCellCount - 1) && scrollOffset + (InventoryCellCount - 1) < (categoryItems.Count - 1)) {
                    scrollOffset += 1;
                    itemCursor -= 1;
                } else if (delta < 0 && itemCursor == 0 && scrollOffset > 0) {
                    scrollOffset -= 1;
                    itemCursor += 1;
                }
            }
            UpdateEntry();
        }

        UpdateDisplay();
    }

    private void Select() {

    }
}
