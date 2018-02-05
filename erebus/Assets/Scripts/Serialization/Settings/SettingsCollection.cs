using UnityEngine;
using System.Collections.Generic;

public class SettingsCollection : MonoBehaviour {

    private const string DefaultsFileName = "Settings/SettingsDefaults";

    private Dictionary<string, Setting<float>> floatSettings;
    private Dictionary<string, Setting<bool>> boolSettings;

    public void Awake() {
        floatSettings = new Dictionary<string, Setting<float>>();
        boolSettings = new Dictionary<string, Setting<bool>>();
    }

    public SettingsMemory ToMemory() {
        SettingsMemory memory = new SettingsMemory();
        foreach (string key in floatSettings.Keys) {
            memory.floatKeys.Add(key);
        }
        foreach (Setting<float> setting in floatSettings.Values) {
            memory.floatValues.Add(setting.Value);
        }
        foreach (string key in boolSettings.Keys) {
            memory.boolKeys.Add(key);
        }
        foreach (Setting<bool> setting in boolSettings.Values) {
            memory.boolValues.Add(setting.Value);
        }
        return memory;
    }

    public void PopulateFromMemory(SettingsMemory memory) {
        floatSettings.Clear();
        for (int i = 0; i < memory.floatKeys.Count; i += 1) {
            AddFloatSetting(memory.floatKeys[i], memory.floatValues[i]);
        }
        for (int i = 0; i < memory.boolKeys.Count; i += 1) {
            AddBoolSetting(memory.boolKeys[i], memory.boolValues[i]);
        }
    }

    public void LoadDefaults() {
        SettingsDefaults defaults = Resources.Load<SettingsDefaults>(DefaultsFileName);
        AddFloatSetting(SettingsConstants.TextSpeed, defaults.textSpeed);
        AddFloatSetting(SettingsConstants.BGMVolume, defaults.bgmVolume);
        AddBoolSetting(SettingsConstants.SkipUnreadText, defaults.skipUnreadText);
        AddFloatSetting(SettingsConstants.SoundEffectVolume, defaults.soundEffectVolume);
        AddFloatSetting(SettingsConstants.AutoSpeed, defaults.autoSpeed);
        AddBoolSetting(SettingsConstants.SkipAtChoices, defaults.skipAtChoices);
        AddBoolSetting(SettingsConstants.Fullscreen, defaults.fullscreen);
    }

    public Setting<float> GetFloatSetting(string tag) {
        if (floatSettings.ContainsKey(tag)) {
            return floatSettings[tag];
        } else {
            return null;
        }
    }

    public Setting<bool> GetBoolSetting(string tag) {
        if (boolSettings.ContainsKey(tag)) {
            return boolSettings[tag];
        } else {
            return null;
        }
    }

    private void AddFloatSetting(string tag, float value) {
        floatSettings.Add(tag, new Setting<float>(tag, value));
    }

    private void AddBoolSetting(string tag, bool value) {
        boolSettings.Add(tag, new Setting<bool>(tag, value));
    }
}
