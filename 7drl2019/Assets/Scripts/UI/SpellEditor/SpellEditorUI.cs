using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class SpellEditorUI : MonoBehaviour {

    public ItemScrollBox bookBox;
    public ItemScrollBox scrollBox;

    [Space]
    public Textbox textbox;
    public Text descriptionNameBox;
    public Text levelText;

    [Space]
    public Text eraserLabel;
    public Text scissorsLabel;

    [Space]
    public Facebox rightFace;
    public Sprite bookFace;
    public Sprite scrollFace;

    private PCEvent pc;
    private RogueUI ui;
    private List<ItemScrollBox.Data> bookData;

    public void Start() {
        descriptionNameBox.transform.parent.GetComponent<CanvasGroup>().alpha = 0.0f;
        GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    public IEnumerator ActivateRoutine(RogueUI ui, PCEvent pc, int level) {
        this.pc = pc;
        this.ui = ui;

        rightFace.hpBar.gameObject.SetActive(false);
        rightFace.mpBar.gameObject.SetActive(false);
        rightFace.cdBar.gameObject.SetActive(false);

        PopulateBookData();
        bookBox.Populate(bookData);
        
        levelText.text = "Cleared floor " + level + " with " + pc.gold + " gold. " +
            "Craft spellbooks and challenge next floor!";
        PopulateTools();

        ui.rightDisplayEnabled = true;
        UpdateDescriptionForSelectedBook();
        yield return CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f));
        yield return textbox.EnableRoutine(null, false);
        descriptionNameBox.transform.parent.GetComponent<CanvasGroup>().alpha = 1.0f;
        yield return SelectBookRoutine();
    }

    public IEnumerator SelectBookRoutine() {
        PopulateBookData();
        UpdateScrollBoxForSelection();

        Result<int> selectResult = new Result<int>();
        yield return bookBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedBook();
        });
    }

    private void UpdateScrollBoxForSelection() {
        List<ItemScrollBox.Data> scrollData = new List<ItemScrollBox.Data>();
        Spellbook activeBook = pc.books[bookBox.selection];
        foreach (Skill skill in activeBook.spells) {
            ItemScrollBox.Data data = new ItemScrollBox.Data();
            data.sprite = skill.icon;
            data.text = skill.skillName + " [" + skill.pageCost + "pg]";
            data.tint = skill.school.Tint();
            scrollData.Add(data);
        }
        scrollBox.Populate(scrollData);
    }

    private void PopulateBookData() {
        bookData = new List<ItemScrollBox.Data>();
        foreach (Spellbook book in pc.books) {
            ItemScrollBox.Data data = new ItemScrollBox.Data();
            PopulateBookCell(data, book);
            bookData.Add(data);
        }
        bookBox.Populate(bookData);
    }

    private void PopulateBookCell(ItemScrollBox.Data data, Spellbook book) {
        data.sprite = book.sprite;
        data.text = book.bookName;
        if (pc.activeBook == book) {
            data.text += " [EQP]";
        }
        data.text += " [" + book.pagesFilled + "/" + book.totalPages + "]";

        data.tint = Color.white;
    }

    private void PopulateTools() {
        eraserLabel.text = "x" + pc.erasers;
        scissorsLabel.text = "x" + pc.scissors;
    }

    private void UpdateDescriptionForSelectedBook() {
        Spellbook book = pc.books[bookBox.selection];
        descriptionNameBox.text = book.bookName;
        textbox.textbox.text = "A spellbook labeled \"" + book.bookName + "\"";
        textbox.textbox.text = "It has " + book.pagesFilled + " pages filled with spells, and another " +
            (book.totalPages - book.pagesFilled) + " pages of scrolls can be added, for a total capacity of " +
            book.totalPages + " pages.";
        if (pc.activeBook == book) {
            textbox.textbox.text += " It is equipped as the current spell loadout.";
        } else {
            textbox.textbox.text += " It must be equipped as the next floor's spell loadout to use.";
        }
        rightFace.face.sprite = bookFace;

        UpdateScrollBoxForSelection();
    }
}
