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
        return bookName;
    }
}
