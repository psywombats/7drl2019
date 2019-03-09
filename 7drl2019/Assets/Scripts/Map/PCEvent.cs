using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BattleEvent))]
public class PCEvent : MonoBehaviour {

    public SpellbookData starterBookData;

    public SpriteRenderer pickup;

    public MapEvent parent { get { return GetComponent<MapEvent>(); } }
    public BattleController battle { get { return GetComponent<BattleEvent>().unit.battle; } }
    public BattleUnit unit { get { return GetComponent<BattleEvent>().unit; } }
    
    public List<Spellbook> books { get; private set; }
    public List<Scroll> scrolls { get; private set; }
    public Spellbook activeBook { get; set; }
    public int gold { get; private set; }
    public int erasers { get;  set; }
    public int scissors { get;  set; }

    public void Awake() {
        scissors = 2;
        erasers = 2;
        books = new List<Spellbook>();
        scrolls = new List<Scroll>();

        Spellbook starter = new Spellbook(starterBookData);
        books.Add(starter);
        activeBook = starter;
    }

    public void Start() {
        Global.Instance().Maps.pc = this;
    }

    public void PickUpItem(Item item, int quantity) {
        // 7drl megahack
        if (item is Scroll) {
            scrolls.Add((Scroll)item);
        } else if (item is SpellbookData) {
            books.Add(new Spellbook((SpellbookData)item));
        } else if (item.isEraser) {
            erasers += quantity;
        } else if (item.isScissors) {
            scissors += quantity;
        } else if (item.isGold) {
            gold += quantity;
        }
    }
}
