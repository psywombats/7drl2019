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
    public Sprite chestFace;

    private PCEvent pc;
    private RogueUI ui;
    private List<ItemScrollBox.Data> bookData;

    private Result<bool> confirmResult;
    private bool ownScrolls;

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
        bookBox.selection = 0;
        PopulateScrollBoxForSelectedBook();
        
        levelText.text = "Cleared floor " + level + " with " + pc.gold + " gold. " +
            "Craft spellbooks and challenge next floor! Press [ESC] when done.";
        PopulateTools();

        textbox.textbox.GetComponent<CanvasGroup>().alpha = 1.0f;

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
        PopulateScrollBoxForSelectedBook();

        ui.rightDisplayEnabled = false;
        yield return textbox.DisableRoutine();
        yield return CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 1.0f));
    }

    public IEnumerator SelectBookRoutine() {
        bool active = true;
        while (active) {
            scrollBox.ClearSelection();
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
                    active = false;
                }
            } else {
                switch (selectResult.value) {
                    case InputManager.Command.Confirm:
                        if (scrollBox.items.Count == 0) {
                            textbox.textbox.text = "No scrolls there.";
                        } else {
                            yield return BrowseRoutine();
                        }
                        break;
                    case InputManager.Command.AddPage:
                        if (pc.scrolls.Count == 0) {
                            textbox.textbox.text = "Pri doesn't own any scrolls. Go find some!";
                        } else if (bookBox.selection < pc.books.Count) {
                            yield return AddPageRoutine();
                        }
                        break;
                    case InputManager.Command.ErasePage:
                        if (bookBox.selection < pc.books.Count) {
                            if (pc.erasers == 0) {
                                textbox.textbox.text = "This requires a magic eraser. Pri doesn't have any.";
                            } else if (pc.books[bookBox.selection].spells.Count == 0) {
                                textbox.textbox.text = "No scrolls to remove.";
                            } else {
                                yield return ErasePageRoutine();
                            }
                        }
                        break;
                    case InputManager.Command.CutPage:
                        if (bookBox.selection < pc.books.Count) {
                            if (pc.scissors == 0) {
                                textbox.textbox.text = "This requires a pair of magic scissors. Pri doesn't have any.";
                            } else if (pc.books[bookBox.selection].spells.Count == 0) {
                                textbox.textbox.text = "No scrolls to extract.";
                            } else {
                                yield return CutPageRoutine();
                            }
                        }
                        break;
                    case InputManager.Command.Equip:
                        if (bookBox.selection < pc.books.Count) {
                            yield return EquipRoutine();
                        } else {
                            textbox.textbox.text = "This is a sack full of scrolls, not a spellbook. Go find a book!";
                        }
                        break;
                }
            }
        }
    }

    private IEnumerator BrowseRoutine() {
        UpdateDescriptionForSelectedScroll();
        Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
        yield return scrollBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedScroll();
        });
        UpdateDescriptionForSelectedBook();
    }

    private IEnumerator EquipRoutine() {
        pc.activeBook = pc.books[bookBox.selection];
        pc.books.Remove(pc.activeBook);
        pc.books.Insert(0, pc.activeBook);
        textbox.textbox.text = "Pri is now equipped with \"" + pc.activeBook.bookName + "\".";
        yield return CoUtils.Wait(0.3f);
        PopulateBookData();
    }

    private IEnumerator AddPageRoutine() {
        PopulateScrollBoxForOwnScrolls();
        UpdateDescriptionForSelectedScroll();
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
                    pc.books[bookBox.selection].bookName + "\"? This is not easily reversible.";
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
        UpdateDescriptionForSelectedScroll();
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
                textbox.textbox.text = "Extract scroll of " + scroll.skill.skillName + " from \"" +
                    pc.books[bookBox.selection].bookName + "\"? This will destroy the spellbook.";
                Result<bool> confirmResult = new Result<bool>();
                yield return ConfirmRoutine(confirmResult);
                if (confirmResult.value) {
                    pc.scrolls.Add(scroll);
                    pc.books.RemoveAt(bookBox.selection);
                    PopulateScrollBoxForSelectedBook();
                    PopulateBookData();
                    UpdateDescriptionForSelectedBook();
                    pc.scissors -= 1;
                    PopulateTools();
                } else {
                    PopulateScrollBoxForSelectedBook();
                }
            }
        }
    }

    private IEnumerator ErasePageRoutine() {
        UpdateDescriptionForSelectedScroll();
        Result<InputManager.Command> selectResult = new Result<InputManager.Command>();
        yield return scrollBox.SelectRoutine(selectResult, (int selection) => {
            UpdateDescriptionForSelectedScroll();
        });
        if (selectResult.canceled) {
            UpdateDescriptionForSelectedBook();
        } else {
            Scroll scroll = pc.activeBook.spells[scrollBox.selection].scroll;
            Spellbook book = pc.books[bookBox.selection];
            descriptionNameBox.text = "Really?";
            textbox.textbox.text = "Erase the scroll of " + scroll.skill.skillName + " from \"" +
                pc.books[bookBox.selection].bookName + "\"? This will destroy the scroll.";
            Result<bool> confirmResult = new Result<bool>();
            yield return ConfirmRoutine(confirmResult);
            if (confirmResult.value) {
                book.RemoveIndex(scrollBox.selection);
                PopulateBookData();
                PopulateScrollBoxForSelectedBook();
                UpdateDescriptionForSelectedBook();
                pc.erasers -= 1;
                PopulateTools();
            } else {
                PopulateScrollBoxForSelectedBook();
                UpdateDescriptionForSelectedBook();
            }
        }
    }

    private void PopulateScrollBoxForSelectedBook() {
        if (bookBox.selection >= pc.books.Count) {
            PopulateScrollBoxForOwnScrolls();
        } else if (bookBox.selection >= 0) {
            ownScrolls = false;
            List<ItemScrollBox.Data> scrollData = new List<ItemScrollBox.Data>();
            Spellbook activeBook = pc.books[bookBox.selection];
            foreach (Skill skill in activeBook.spells) {
                ItemScrollBox.Data data = new ItemScrollBox.Data();
                PopulateScrollCell(data, skill.scroll);
                scrollData.Add(data);
            }
            scrollBox.Populate(scrollData);
        }
    }

    private void PopulateBookData() {
        scrollBox.ClearSelection();
        bookData = new List<ItemScrollBox.Data>();
        foreach (Spellbook book in pc.books) {
            ItemScrollBox.Data data = new ItemScrollBox.Data();
            PopulateBookCell(data, book);
            bookData.Add(data);
        }
        ItemScrollBox.Data specialData = new ItemScrollBox.Data();
        specialData.text = "Miscellaneous scrolls";
        specialData.tint = Color.white;
        specialData.sprite = null;
        bookData.Add(specialData);
        bookBox.Populate(bookData);

        UpdateDescriptionForSelectedBook();
    }

    private void PopulateScrollBoxForOwnScrolls() {
        ownScrolls = true;
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
        data.tint = scroll.skill.school.Tint();
    }

    private void PopulateTools() {
        eraserLabel.text = "x" + pc.erasers;
        scissorsLabel.text = "x" + pc.scissors;
    }

    private void UpdateDescriptionForSelectedBook() {
        if (bookBox.selection >= pc.books.Count) {
            descriptionNameBox.text = "Looseleaf scrolls";
            textbox.textbox.text = "Miscellaneous scrolls owned by Pri. They must be added to a spellbook before they" +
                " can be cast.";
            textbox.textbox.text += "\n\nPress [SPACE] to browse the scrolls.";

            rightFace.face.sprite = chestFace;
        } else {
            Spellbook book = pc.books[bookBox.selection > 0 ? bookBox.selection : 0];
            descriptionNameBox.text = book.bookName;
            textbox.textbox.text = "A spellbook labeled \"" + book.bookName + "\". ";
            textbox.textbox.text += "It has " + book.pagesFilled + " pages filled with spells, and another " +
                (book.totalPages - book.pagesFilled) + " pages of scrolls can be added, for a total capacity of " +
                book.totalPages + " pages.";
            if (pc.activeBook == book) {
                textbox.textbox.text += " Pri currently has this spellbook equipped as the current loadout.";
            } else {
                textbox.textbox.text += " Pri must equip this book as the loadout for its spells to be used.";
            }
            textbox.textbox.text += "\n\nPress [SPACE] to browse the book, or press: [E]quip the book, [A]dd a scroll, " +
                "e[X]tract a scroll (destroying the book), [R]emove a scroll (destroying the scroll).";

            rightFace.face.sprite = bookFace;
        }

        textbox.textbox.text.Replace("\\n", "\n");
        PopulateScrollBoxForSelectedBook();
    }

    private void UpdateDescriptionForSelectedScroll() {
        Skill skill;
        if (ownScrolls) {
            skill = pc.scrolls[scrollBox.selection > 0 ? scrollBox.selection : 0].skill;
        } else {
            Spellbook book = pc.books[bookBox.selection];
            skill = book.spells[scrollBox.selection > 0 ? scrollBox.selection : 0];
        }
        descriptionNameBox.text = skill.skillName;
        textbox.textbox.text = skill.data.description;
        textbox.textbox.text += "\n\nThe casting instructions for " + skill.skillName + " are printed across " +
            skill.pageCost + " pages.";
        foreach (SkillModifier mod in skill.mods) {
            textbox.textbox.text += " " + mod.DescriptiveString();
        }
        if (skill.costMP > 0) {
            textbox.textbox.text += " Casting requires " + skill.costMP + " magic.";
        } else {
            textbox.textbox.text += " Casting will put this and similar spells under a cooldown until " + skill.costCD +
                " monsters are slain.";
        }
        textbox.textbox.text.Replace("\\n", "\n");
        rightFace.face.sprite = scrollFace;
    }
    
    private IEnumerator ConfirmRoutine(Result<bool> result) {
        confirmResult = result;
        textbox.textbox.text += "\n\nPress [SPACE] to confirm or [ESC] to cancel";
        textbox.textbox.text.Replace("\\n", "\n");
        Global.Instance().Input.PushListener(this);
        while (!confirmResult.finished) {
            yield return null;
        }
        Global.Instance().Input.RemoveListener(this);
        UpdateDescriptionForSelectedBook();
    }
}
