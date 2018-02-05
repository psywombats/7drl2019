using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class ButtonSoundComponent : MonoBehaviour {
    
    private const string DefaultClickSoundTag = "click";

    public string ClickSoundTag = "click";

    public void Awake() {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            HandleButtonEvent(ClickSoundTag.Length > 0 ? ClickSoundTag : DefaultClickSoundTag);
        });
    }

    private void HandleButtonEvent(string associatedTag) {
        if (gameObject.activeInHierarchy) {
            Global.Instance().Audio.PlaySFX(associatedTag);
        }
    }
}
