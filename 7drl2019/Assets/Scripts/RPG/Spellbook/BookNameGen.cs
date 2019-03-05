using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpellbookNames", menuName = "Data/RPG/Spellbook Name Generator")]
public class BookNameGen : ScriptableObject {

    public List<string> prefixes;
    public List<string> bases;
    public List<string> suffixes;

    public string Generate() {
        return
            prefixes[Random.Range(0, prefixes.Count)] +
            bases[Random.Range(0, bases.Count)] +
            suffixes[Random.Range(0, suffixes.Count)];
    }
}
