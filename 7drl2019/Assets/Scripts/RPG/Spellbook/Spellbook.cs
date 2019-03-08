using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spellbook {

    public List<Skill> spells { get; private set; }
    public int totalPages { get; private set; }
    public string bookName { get; private set; }
    public Sprite sprite { get; private set; }

    public int pagesFilled {
        get {
            int total = 0;
            foreach (Skill spell in spells) {
                total += spell.pageCost;
            }
            return total;
        }
    }

    public Spellbook(SpellbookData data) {
        totalPages = data.pageCount;
        bookName = data.bookName;
        spells = new List<Skill>();
        sprite = data.sprite;
        foreach (Scroll scroll in data.spells) {
            spells.Add(scroll.skill);
        }
    }

    public void AddScroll(Scroll scroll) {
        spells.Add(scroll.skill);
    }

    public void RemoveIndex(int i) {
        spells.RemoveAt(i);
    }
}
