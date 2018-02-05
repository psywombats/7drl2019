using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ConfirmMenuComponent : MenuComponent {

    private const string PrefabName = "UI/ConfirmMenu";

    public struct ConfirmMenuData {
        public string bodyText;
        public string confirmText;
        public string cancelText;
        public Action onConfirm;
        public Action onCancel;
        public Action onFinish;
    };

    public Text bodyText;
    public Text confirmText;
    public Text cancelText;
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    public void Awake() {
        confirmButton.onClick.AddListener(() => {
            onConfirm();
            StartCoroutine(FadeOutRoutine());
        });
        cancelButton.onClick.AddListener(() => {
            onCancel();
            StartCoroutine(FadeOutRoutine());
        });
    }

    public static GameObject Spawn(GameObject parent, ConfirmMenuData data) {
        GameObject menuObject = Spawn(parent, PrefabName, data.onFinish);
        menuObject.GetComponent<ConfirmMenuComponent>().PopulateFromData(data);
        return menuObject;
    }

    private void PopulateFromData(ConfirmMenuData data) {
        bodyText.text = data.bodyText != null ? data.bodyText : bodyText.text;
        confirmText.text = data.confirmText != null ? data.confirmText : confirmText.text;
        cancelText.text = data.cancelText != null ? data.cancelText : cancelText.text;
        onConfirm = data.onConfirm;
        onCancel = data.onCancel;
    }
}
