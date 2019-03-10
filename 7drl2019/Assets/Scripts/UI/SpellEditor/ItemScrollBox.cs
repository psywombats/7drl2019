using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RectTransform))]
public class ItemScrollBox : MonoBehaviour, InputListener {

    public class Data {
        public Sprite sprite;
        public string text;
        public Color tint;
    }

    public List<ItemCell> cells;
    public Image upArrow;
    public Image downArrow;

    public List<Data> items { get; private set; }
    public int selection { get; set; }

    private int offset;
    private Result<InputManager.Command> awaitingResult;
    private Action<int> scanner;

    public void Populate(List<Data> items) {
        this.items = items;

        selection = -1;
        offset = 0;
        for (int i = 0; i < cells.Count; i += 1) {
            int at = offset + i;
            if (at >= items.Count) {
                cells[i].Disable();
            } else {
                cells[i].Populate(items[at]);
            }
            
        }
        UpdateSelectionArrows();
    }

    public void ClearSelection() {
        selection = -1;
        UpdateSelectionArrows();
        UpdateHighlight();
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Up:
                MoveSelection(-1);
                break;
            case InputManager.Command.Down:
                MoveSelection(1);
                break;
            case InputManager.Command.Cancel:
                awaitingResult.Cancel();
                EndAwait();
                break;
            case InputManager.Command.Confirm:
            case InputManager.Command.Equip:
            case InputManager.Command.CutPage:
            case InputManager.Command.ErasePage:
            case InputManager.Command.AddPage:
            case InputManager.Command.View:
                awaitingResult.value = command;
                EndAwait();
                break;
        }
        return true;
    }

    public IEnumerator SelectRoutine(Result<InputManager.Command> result, Action<int> scanner) {
        this.scanner = scanner;
        awaitingResult = result;
        if (selection == -1) {
            selection = 0;
        }
        UpdateHighlight();
        Global.Instance().Input.PushListener(this);
        while (!awaitingResult.finished) {
            yield return null;
        }
    }

    private void UpdateSelectionArrows() {
        if (items != null && items.Count > 0) {
            upArrow.enabled = offset > 0;
            downArrow.enabled = (offset + items.Count) > cells.Count;
        } else {
            upArrow.enabled = false;
            downArrow.enabled = false;
        }
    }

    private void UpdateHighlight() {
        for (int i = 0; i < cells.Count; i += 1) {
            int at = offset + i;
            cells[i].SetSeleceted(at == selection);
        }
    }

    private void EndAwait() {
        scanner = null;
        Global.Instance().Input.RemoveListener(this);
        offset = 0;
        UpdateHighlight();
        UpdateSelectionArrows();
    }

    private void MoveSelection(int delta) {
        selection += delta;
        if (selection < 0) {
            selection = 0;
        }
        if (selection >= items.Count) {
            selection = items.Count - 1;
        }
        if (offset > selection) {
            offset = selection;
        }
        if (offset + cells.Count < selection) {
            offset = selection - cells.Count + 1;
        }
        UpdateHighlight();
        UpdateSelectionArrows();

        scanner(selection);
    }
}
