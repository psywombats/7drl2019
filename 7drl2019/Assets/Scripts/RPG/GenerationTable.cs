using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GenerationTable", menuName = "Data/RPG/Generation Table")]
public class GenerationTable : ScriptableObject {

    public int baseDanger;
    public int dangerPerLevel;
    public int itemsLow, itemsHigh;
    public List<int> firstLevelDangerOverrides;
    public List<int> firstLevelItemCountOverrides;

    [Space]
    public Item gold;
    public Item eraser;
    public Item scissors;
    public SpellbookData book;
    public Scroll scroll;
    public List<Encounter> encounters;
    public List<SkillData> skills;

    public List<Encounter> GenerateEncounters(int level) {
        int targetDanger;
        if (level < firstLevelDangerOverrides.Count) {
            targetDanger = firstLevelDangerOverrides[level];
        } else {
            targetDanger = baseDanger + level * dangerPerLevel;
        }

        List<Encounter> results = new List<Encounter>();
        int currentDanger = 0;
        while (currentDanger < targetDanger) {
            RandUtils.Shuffle(encounters);
            Encounter toAdd = null;
            foreach (Encounter encounter in encounters) {
                if (encounter.danger < targetDanger / 3.5f 
                        && encounter.danger > targetDanger / 20.0f
                        && encounter.danger + currentDanger <= targetDanger * 1.1f
                        && RandUtils.Chance(encounter.rarity / 100.0f)) {
                    toAdd = encounter;
                    currentDanger += encounter.danger;
                    break;
                }
            }
            if (toAdd == null) {
                currentDanger += 50;
            } else {
                results.Add(toAdd);
            }
        }
        return results;
    }

    public List<Item> GenerateItems(int level) {
        int count;
        if (level < firstLevelItemCountOverrides.Count) {
            count = firstLevelItemCountOverrides[level];
        } else {
            count = Random.Range(itemsLow, itemsHigh - itemsLow + 1);
        }

        List<Item> items = new List<Item>();
        while (items.Count < count) {
            Item toAdd = null;
            float r = Random.Range(0.0f, 1.0f);
            if (r < 0.5)        toAdd = gold;
            else if (r < 0.8)   toAdd = GenerateScroll(level);
            else if (r < 0.95)  toAdd = GenerateBook(level);
            else                toAdd = GeneratePickup(level);
            if (toAdd != null) {
                items.Add(toAdd);
            }
        }

        return items;
    }

    private Scroll GenerateScroll(int level) {
        int modCount = 0;
        if (RandUtils.Chance(0.6f)) {
            modCount += 1;
        }
        if (level >= 5 && RandUtils.Chance(0.4f)) {
            modCount += 1;
        }

        List<SkillModifier.Type> toApply = new List<SkillModifier.Type>();
        List<SkillModifier.Type> mods = new List<SkillModifier.Type>(
            (SkillModifier.Type[])System.Enum.GetValues(typeof(SkillModifier.Type)));
        while (toApply.Count < modCount) {
            SkillModifier.Type mod = mods[Random.Range(0, mods.Count)];
            if (!toApply.Contains(mod)) {
                toApply.Add(mod);
            }
        }

        Scroll scroll = Instantiate(this.scroll);
        scroll.mods = toApply;
        while (scroll.data == null) {
            SkillData skill = skills[Random.Range(0, skills.Count)];
            if (RandUtils.Chance((skill.rarity - level) / 100.0f)) {
                scroll.data = skill;
            }
        }

        return scroll;
    }

    private SpellbookData GenerateBook(int level) {
        SpellbookData book = Instantiate(this.book);
        book.spells = new List<Scroll>();
        book.pageCount = 8 + Random.Range(0, Mathf.CeilToInt(level / 3.0f));

        int preSpells;
        float r = Random.Range(0.0f, 1.0f);
        if (r < 0.5)        preSpells = 1;
        else if (r < 0.8)   preSpells = 2;
        else                preSpells = 0;

        while (book.spells.Count < preSpells) {
            book.spells.Add(GenerateScroll(level));
            book.pageCount += 1;
        }

        return book;
    }

    private Item GeneratePickup(int level) {
        if (level < 4) {
            return null;
        }
        if (RandUtils.Chance(0.7f)) {
            return scissors;
        } else {
            return eraser;
        }
    }
}
