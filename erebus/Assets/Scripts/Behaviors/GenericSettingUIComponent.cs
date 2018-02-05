using UnityEngine;

public abstract class GenericSettingUIComponent<T> : SettingUIComponent {

    protected Setting<T> setting;

    protected abstract Setting<T> InitializeSetting(string settingName);

    public override void Awake() {
        setting = InitializeSetting(settingName);
        base.Awake();
    }
}
