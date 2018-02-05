using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Scrollbar))]
public class SettingSliderComponent : GenericSettingUIComponent<float> {
    
    public Text label;
    public bool percentDisplayMode;
    public string[] gradiatedLabels;
    
    private Scrollbar scrollbar;
    private Vector2 originalLabelLocation;
    

    public override void Awake() {
        scrollbar = GetComponent<Scrollbar>();
        originalLabelLocation = label.transform.localPosition;
        base.Awake();
    }

    public void Update() {
        MatchLabelDisplayToScrollbar();
        dirty = Math.Abs(setting.Value - scrollbar.value) > 0.02;
    }

    protected override void MatchDisplayToSetting() {
        scrollbar.value = setting.Value;
        MatchLabelDisplayToScrollbar();
    }

    protected override void MatchSettingToDisplay() {
        setting.Value = scrollbar.value;
    }

    protected override Setting<float> InitializeSetting(string settingName) {
        return Global.Instance().Settings.GetFloatSetting(settingName);
    }

    private void MatchLabelDisplayToScrollbar() {
        if (percentDisplayMode) {
            label.text = Mathf.Round(scrollbar.value * 100.0f) + "%";
        } else {
            int index = Mathf.RoundToInt((float)(gradiatedLabels.Length - 1) * scrollbar.value);
            label.text = gradiatedLabels[index];
        }

        float maximum = scrollbar.GetComponent<RectTransform>().rect.width - (originalLabelLocation.x * 2.0f);
        label.transform.localPosition = new Vector2(originalLabelLocation.x + maximum * scrollbar.value, originalLabelLocation.y);
    }
}
