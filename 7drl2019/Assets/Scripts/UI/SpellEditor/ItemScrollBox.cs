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
    public int selection { get; private set; }

    private int offset;
    private Result<int> awaitingResult;
    private Action<int> scanner;

    public void Populate(List<Data> items) {
        this.items = items;

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
                awaitingResult.value = selection;
                EndAwait();
                break;
        }
        return true;
    }

    public IEnumerator SelectRoutine(Result<int> result, Action<int> scanner) {
        this.scanner = scanner;
        awaitingResult = result;
        selection = 0;
        scanner(selection);
        UpdateHighlight();
        Global.Instance().Input.PushListener(this);
        while (!awaitingResult.finished) {
            yield return null;
        }
    }

    private void UpdateSelectionArrows() {
        upArrow.enabled = offset > 0;
        downArrow.enabled = (offset + items.Count) > cells.Count;
    }

    private void UpdateHighlight() {
        for (int i = 0; i < cells.Count; i += 1) {
            int at = offset + i;
            cells[i].SetSeleceted(at == selection);
        }
    }

    private void EndAwait() {
        awaitingResult = null;
        scanner = null;
        Global.Instance().Input.RemoveListener(this);
        selection = -1;
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
