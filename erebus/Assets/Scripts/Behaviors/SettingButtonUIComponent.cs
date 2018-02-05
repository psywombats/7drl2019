using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingButtonUIComponent : GenericSettingUIComponent<bool> {

    public Toggle yesToggle;
    public Toggle noToggle;

    public override void Awake() {
        base.Awake();
        yesToggle.onValueChanged.AddListener((bool newValue) => {
            noToggle.isOn = !yesToggle.isOn;
            dirty = true;
        });
        noToggle.onValueChanged.AddListener((bool newValue) => {
            yesToggle.isOn = !noToggle.isOn;
            dirty = true;
        });
    }

    protected override Setting<bool> InitializeSetting(string settingName) {
        return Global.Instance().Settings.GetBoolSetting(settingName);
    }

    protected override void MatchDisplayToSetting() {
        yesToggle.isOn = setting.Value;
        noToggle.isOn = !setting.Value;
    }

    protected override void MatchSettingToDisplay() {
        setting.Value = yesToggle.isOn;
    }
}
