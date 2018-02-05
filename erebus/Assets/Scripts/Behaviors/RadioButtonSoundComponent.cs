using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Toggle))]
public class RadioButtonSoundComponent : MonoBehaviour {
    
    private const string DefaultClickSoundTag = "click";

    public string ClickSoundTag = "";

    public void Awake() {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((bool value) => {
            HandleButtonEvent(ClickSoundTag.Length > 0 ? ClickSoundTag : DefaultClickSoundTag);
        });
    }

    private void HandleButtonEvent(string associatedTag) {
        Global.Instance().Audio.PlaySFX(associatedTag);
    }
}
