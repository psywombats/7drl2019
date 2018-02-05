using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveButtonComponent : MonoBehaviour {

    public Button button;
    public Text dataText;
    public Text captionText;
    public Image screenshot;

    private SaveMenuComponent menu;
    private int slot;

    public void Awake() {
        button.onClick.AddListener(() => {
            menu.SaveOrLoadFromSlot(slot);
        });
    }

    public void Populate(SaveMenuComponent menu, int slot, Memory memory, SaveMenuComponent.SaveMenuMode mode) {
        this.menu = menu;
        this.slot = slot;

        dataText.text = "slot0" + (slot + 1);
        if (memory == null) {
            if (mode == SaveMenuComponent.SaveMenuMode.Load) {
                button.enabled = false;
                dataText.text = "<no data>";
            }
            captionText.text = "";
            screenshot.gameObject.SetActive(false);
        } else {
            captionText.text = System.String.Format("{0:g}", Utils.TimestampToDateTime(memory.savedAt));
            screenshot.overrideSprite = Global.Instance().memory.SpriteFromBase64(memory.base64ScreenshotPNG);
            screenshot.gameObject.SetActive(true);
        }
    }
}
