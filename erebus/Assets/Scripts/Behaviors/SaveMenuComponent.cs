using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class SaveMenuComponent : MenuComponent {

    private const string PrefabName = "UI/SaveMenu";
    private const float FadeoutSeconds = 0.2f;

    public enum SaveMenuMode {
        Save, Load
    };

    public SaveButtonComponent[] slots;
    
    private SaveMenuMode mode;
    
    public static GameObject Spawn(GameObject parent, SaveMenuMode mode, Action onFinish) {
        GameObject menuObject = Spawn(parent, PrefabName, onFinish);
        menuObject.GetComponent<SaveMenuComponent>().mode = mode;
        return menuObject;
    }

    public override void Start() {
        base.Start();
        RefreshData();
    }

    public void SaveOrLoadFromSlot(int slot) {
        if (mode == SaveMenuMode.Load) {
            Memory memory = Global.Instance().memory.GetMemoryForSlot(slot);
            StartCoroutine(LoadRoutine(memory));
        } else {
            Global.Instance().memory.SaveToSlot(slot);
            RefreshData();
            StartCoroutine(ResumeRoutine());
        }
    }

    protected override void SetInputEnabled(bool enabled) {
        base.SetInputEnabled(enabled);
        foreach (SaveButtonComponent button in slots) {
            button.button.interactable = enabled;
        }
    }

    private void RefreshData() {
        for (int i = 0; i < slots.Length; i += 1) {
            SaveButtonComponent saveButton = slots[i];
            Memory memory = Global.Instance().memory.GetMemoryForSlot(i);
            saveButton.Populate(this, i, memory, mode);
        }
    }

    private IEnumerator LoadRoutine(Memory memory) {
        SetInputEnabled(false);
        yield return StartCoroutine(FadeOutRoutine());
        Global.Instance().input.RemoveListener(this);
        Global.Instance().memory.LoadMemory(memory);
    }
}
