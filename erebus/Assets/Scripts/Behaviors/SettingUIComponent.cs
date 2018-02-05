using UnityEngine;

public abstract class SettingUIComponent : MonoBehaviour {

    public string settingName;

    protected bool dirty;

    public virtual void Awake() {
        MatchDisplayToSetting();
        dirty = false;
    }

    public void Apply() {
        dirty = false;
        MatchSettingToDisplay();
    }

    public bool IsDirty() {
        return dirty;
    }

    protected abstract void MatchDisplayToSetting();

    protected abstract void MatchSettingToDisplay();
}
