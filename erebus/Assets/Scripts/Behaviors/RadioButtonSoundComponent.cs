using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Toggle))]
public class RadioButtonSoundComponent : MonoBehaviour {
    
    private const string DefaultClickSoundTag = "click";

    public string ClickSoundTag = "";

    private SoundPlayer player;

    public void Awake() {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((bool value) => {
            HandleButtonEvent(ClickSoundTag.Length > 0 ? ClickSoundTag : DefaultClickSoundTag);
        });

        player = FindObjectOfType<SoundPlayer>();
    }

    private void HandleButtonEvent(string associatedTag) {
        player.PlaySound(associatedTag);
    }
}
