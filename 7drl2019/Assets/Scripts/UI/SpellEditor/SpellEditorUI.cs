using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class SpellEditorUI : MonoBehaviour, InputListener {

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

    private Result<bool> confirmResult;

    public void Start() {
        descriptionNameBox.transform.parent.GetComponent<CanvasGroup>().alpha = 0.0f;
        GetComponent<CanvasGroup>().alpha = 0.0f;
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        if (command == InputManager.Command.Confirm) {
            confirmResult.value = true;
        } else if (command == InputManager.Command.Cancel) {
            confirmResult.value = false;
        }
        return true;
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
            "Craft spellbooks and challenge next floor! Press [ESC] when done.";
        PopulateTools();

        ui.rightDisplayEnabled = true;
        UpdateDescriptionForSelectedBook();
        yield return CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f));
        yield return textbox.EnableRoutine(null, false);
        descriptionNameBox.transform.parent.GetComponent<CanvasGroup>().alpha = 1.0f;
        yield return SelectBookRoutine();
    }

    public IEnumerator DeactivateRoutine() {
        rightFace.hpBar.gameObject.SetActive(true);
        rightFace.mpBar.gameObject.SetActive(true);
        rightFace.cdBar.gameObject.SetActive(true);

        PopulateBookData();
        bookBox.Populate(bookData);
        UpdateScrollBoxForSelection();

        ui.rightDisplayEnabled = false;
        yield return textbox.DisableRoutine();
        yield return CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 1.0f));
    }

    public IEnumerator SelectBookRoutine() {
        while (true) {
            Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
            yield return bookBox.SelectRoutine(selectResult, (int selection) => {
                UpdateDescriptionForSelectedBook();
            });

            if (selectResult.canceled) {
                descriptionNameBox.text = "Ready?";
                textbox.textbox.text = "Pri is currently equipped with \"" + pc.activeBook.bookName +
                    "\" as the current loadout. Ready to go?";
                Result<bool> confirmResult = new Result<bool>();
                yield return ConfirmRoutine(confirmResult);
                if (confirmResult.value) {
                    yield return DeactivateRoutine();
                    break;
                }
            } else {
                switch (selectResult.value) {
                    case InputManager.Command.AddPage:
                        yield return AddPageRoutine();
                        break;
                    case InputManager.Command.ErasePage:
                        if (pc.erasers == 0) {
                            textbox.textbox.text = "This requires a magic eraser. Pri doesn't have any.";
                        } else {
                            yield return ErasePageRoutine();
                        }
                        break;
                    case InputManager.Command.CutPage:
                        if (pc.scissors == 0) {
                            textbox.textbox.text = "This requires a pair of magic scissors. Pri doesn't have any.";
                        } else {
                            yield return CutPageRoutine();
                        }
                        break;
                    case InputManager.Command.Equip:
                        yield return EquipRoutine();
                        break;
                }
            }
        }
    }

    private IEnumerator EquipRoutine() {
        pc.activeBook = pc.books[bookBox.selection];
        pc.books.Remove(pc.activeBook);
        pc.books.Insert(0, pc.activeBook);
        textbox.textbox.text = "Pri is now equipped with\"" + pc.activeBook.bookName + "\".";
        yield return CoUtils.Wait(0.3f);
        PopulateBookData();
    }

    private IEnumerator AddPageRoutine() {
        PopulateOwnScrollData();
        Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
        yield return scrollBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedScroll();
        });
        if (selectResult.canceled) {
            UpdateDescriptionForSelectedBook();
        } else {
            Scroll scroll = pc.scrolls[scrollBox.selection];
            Spellbook book = pc.books[bookBox.selection];
            if (scroll.skill.pageCost + book.pagesFilled > book.totalPages) {
                textbox.textbox.text = "That scroll is too many pages long.";
            } else {
                descriptionNameBox.text = "Really?";
                textbox.textbox.text = "Add scroll of " + scroll.skill.skillName + " to \"" +
                    pc.books[bookBox.selection] + "\"? This is not easily reversible.";
                Result<bool> confirmResult = new Result<bool>();
                yield return ConfirmRoutine(confirmResult);
                if (confirmResult.value) {
                    pc.scrolls.Remove(scroll);
                    book.AddScroll(scroll);
                    UpdateDescriptionForSelectedBook();
                } else {
                    UpdateDescriptionForSelectedBook();
                }
            }
        }
    }

    private IEnumerator CutPageRoutine() {
        Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
        yield return scrollBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedScroll();
        });
        if (selectResult.canceled) {
            UpdateDescriptionForSelectedBook();
        } else {
            Scroll scroll = pc.activeBook.spells[scrollBox.selection].scroll;
            Spellbook book = pc.books[bookBox.selection];
            if (pc.activeBook == book) {
                textbox.textbox.text = "Can't destroy the equipped spellbook.";
            } else {
                descriptionNameBox.text = "Really?";
                textbox.textbox.text = "Cut scroll of " + scroll.skill.skillName + " from \"" +
                    pc.books[bookBox.selection] + "\"? This will destroy the spellbook.";
                Result<bool> confirmResult = new Result<bool>();
                yield return ConfirmRoutine(confirmResult);
                if (confirmResult.value) {
                    pc.scrolls.Add(scroll);
                    pc.books.RemoveAt(bookBox.selection);
                    UpdateScrollBoxForSelection();
                    PopulateBookData();
                    UpdateDescriptionForSelectedBook();
                    pc.scissors -= 1;
                } else {
                    UpdateScrollBoxForSelection();
                }
            }
        }
    }

    private IEnumerator ErasePageRoutine() {
        Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
        yield return scrollBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedBook();
        });
        if (selectResult.canceled) {
            UpdateScrollBoxForSelection();
        } else {
            Scroll scroll = pc.activeBook.spells[scrollBox.selection].scroll;
            Spellbook book = pc.books[bookBox.selection];
            descriptionNameBox.text = "Really?";
            textbox.textbox.text = "Erase the scroll of " + scroll.skill.skillName + " from \"" +
                pc.books[bookBox.selection] + "\"? This will destroy the scroll.";
            Result<bool> confirmResult = new Result<bool>();
            yield return ConfirmRoutine(confirmResult);
            if (confirmResult.value) {
                book.RemoveIndex(scrollBox.selection);
                UpdateScrollBoxForSelection();
                UpdateDescriptionForSelectedBook();
                pc.erasers -= 1;
            } else {
                UpdateScrollBoxForSelection();
                UpdateDescriptionForSelectedBook();
            }
        }
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

    private void PopulateOwnScrollData() {
        List<ItemScrollBox.Data> scrollData = new List<ItemScrollBox.Data>();
        foreach (Scroll scroll in pc.scrolls) {
            ItemScrollBox.Data data = new ItemScrollBox.Data();
            PopulateScrollCell(data, scroll);
            scrollData.Add(data);
        }
        scrollBox.Populate(scrollData);
    }

    private void PopulateBookCell(ItemScrollBox.Data data, Spellbook book) {
        data.sprite = book.sprite;
        data.text = book.bookName;
        data.text += " [" + book.pagesFilled + "/" + book.totalPages + "]";
        if (pc.activeBook == book) {
            data.text += " [EQP]";
        }
        data.tint = Color.white;
    }

    private void PopulateScrollCell(ItemScrollBox.Data data, Scroll scroll) {
        data.sprite = scroll.skill.icon;
        data.text = scroll.skill.longformName;
        data.tint = Color.white;
    }

    private void PopulateTools() {
        eraserLabel.text = "x" + pc.erasers;
        scissorsLabel.text = "x" + pc.scissors;
    }

    private void UpdateDescriptionForSelectedBook() {
        Spellbook book = pc.books[bookBox.selection];
        descriptionNameBox.text = book.bookName;
        textbox.textbox.text = "A spellbook labeled \"" + book.bookName + "\" ";
        textbox.textbox.text += "It has " + book.pagesFilled + " pages filled with spells, and another " +
            (book.totalPages - book.pagesFilled) + " pages of scrolls can be added, for a total capacity of " +
            book.totalPages + " pages.";
        if (pc.activeBook == book) {
            textbox.textbox.text += " Pri currently has this spellbook equipped as the current loadout.";
        } else {
            textbox.textbox.text += " Pri must equip this book as the loadout for its spells to be used.";
        }
        textbox.textbox.text += "Press [E] to equip, [A] to add a scroll, [X] to extract a scroll " +
            "(and destroy the book), or [R] to remove a scroll (and destroy it).";

        rightFace.face.sprite = bookFace;
        UpdateScrollBoxForSelection();
    }

    private void UpdateDescriptionForSelectedScroll() {
        Skill skill = pc.activeBook.spells[scrollBox.selection];
        descriptionNameBox.text = skill.skillName;
        textbox.textbox.text = skill.data.description;
        textbox.textbox.text += "<br><br>The casting instructions for " + skill.skillName + " are printed across " +
            skill.pageCost + " pages. ";
        foreach (SkillModifier mod in skill.mods) {
            textbox.textbox.text += " " + mod.DescriptiveString();
        }
        if (skill.costMP > 0) {
            textbox.textbox.text += " Casting requires " + skill.costMP + " magic.";
        } else {
            textbox.textbox.text += " Casting will put this and similar spells under a cooldown until " + skill.costCD +
                " monsters are slain.";
        }
    }
    
    private IEnumerator ConfirmRoutine(Result<bool> result) {
        confirmResult = result;
        textbox.textbox.text += "<br><br>Press [SPACE] to confirm or [ESC] to cancel";
        while (!confirmResult.finished) {
            yield return null;
        }
    }
}
