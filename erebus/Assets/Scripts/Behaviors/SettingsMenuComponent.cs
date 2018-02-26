using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SettingsMenuComponent : MenuComponent {

    private const string PrefabName = "Prefabs/UI/SettingsMenu";
    private const string ConfirmBodyText = "Save changes?";
    private const string AffirmText = "Apply";
    private const string CancelText = "Close";
    private const float FadeoutSeconds = 0.2f;

    public SettingUIComponent[] settings;
    public Button cancelButton;
    public Button applyButton;

    public void Awake() {
        cancelButton.onClick.AddListener(() => {
            Cancel();
        });
        applyButton.onClick.AddListener(() => {
            Apply();
        });
    }

    public static GameObject Spawn(GameObject parent, Action onFinish) {
        return Spawn(parent, PrefabName, onFinish);
    }

    public override bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        switch (command) {
            case InputManager.Command.Menu:
            case InputManager.Command.Rightclick:
                Cancel();
                return true;
            default:
                return false;
        }
    }

    private void Apply() {
        foreach (SettingUIComponent setting in settings) {
            setting.Apply();
        }
        Global.Instance().Memory.SaveSystemMemory();
        StartCoroutine(ResumeRoutine());
    }

    private void Cancel() {
        bool dirty = false;
        foreach (SettingUIComponent setting in settings) {
            if (setting.IsDirty()) {
                dirty = true;
                break;
            }
        }
        if (dirty) {
            ConfirmMenuComponent.ConfirmMenuData data = new ConfirmMenuComponent.ConfirmMenuData();
            data.bodyText = ConfirmBodyText;
            data.confirmText = AffirmText;
            data.cancelText = CancelText;
            data.onConfirm = () => {
                Apply();
            };
            data.onCancel = () => {
                StartCoroutine(ResumeRoutine());
            };
            GameObject menuObject = ConfirmMenuComponent.Spawn(gameObject, data);
            menuObject.GetComponent<ConfirmMenuComponent>().Alpha = 0.0f;
            StartCoroutine(menuObject.GetComponent<ConfirmMenuComponent>().FadeInRoutine());
        } else {
            StartCoroutine(ResumeRoutine());
        }
    }
}
