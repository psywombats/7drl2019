using UnityEngine;

[CreateAssetMenu(fileName = "SettingsDefaults", menuName = "Data/SettingsDefaults")]
public class SettingsDefaults : ScriptableObject {

    public float textSpeed;
    public float bgmVolume;
    public float soundEffectVolume;
    public float autoSpeed;
    public bool skipUnreadText;
    public bool skipAtChoices;
    public bool fullscreen;

}
