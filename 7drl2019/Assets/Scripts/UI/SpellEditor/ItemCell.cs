using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ItemCell : MonoBehaviour {

    public RectTransform selectedBacker;
    public Image icon;
    public Text label;

    public void Populate(ItemScrollBox.Data item, bool selected = false) {
        icon.enabled = true;
        icon.sprite = item.sprite;
        if (icon.sprite == null) {
            icon.enabled = false;
        }
        icon.color = item.tint;
        label.text = item.text;
        SetSeleceted(selected);
    }

    public void SetSeleceted(bool selected) {
        selectedBacker.gameObject.SetActive(selected);
    }

    public void Disable() {
        icon.enabled = false;
        label.text = "";
        SetSeleceted(false);
    }
}
