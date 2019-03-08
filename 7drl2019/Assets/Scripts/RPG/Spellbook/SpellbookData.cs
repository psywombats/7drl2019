using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Spellbook", menuName = "Data/RPG/Spellbook")]
public class SpellbookData : Item {

    private const string BookNameGenPath = "";

    public List<Scroll> spells;
    public string bookName;
    public int pageCount;

    public override string ItemName() {
        if (bookName == null || bookName.Length == 0) {
            BookNameGen gen = Resources.Load<BookNameGen>("Database/SpellbookNames");
            bookName = gen.Generate();
        }

        int usedPages = 0;
        foreach (Scroll scroll in spells) {
            int basePg = scroll.data.basePages;
            foreach (SkillModifier.Type type in scroll.mods) {
                SkillModifier mod = new SkillModifier(type);
                basePg = mod.MutatePages(basePg);
            }
            usedPages += basePg;
        }
        return "\"" + bookName + "\" [ " + usedPages + "/" + pageCount + "pg ]";
    }
}
