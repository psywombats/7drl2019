using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class PortraitGroupComponent : MonoBehaviour {

    public TachiComponent portraitA;
    public TachiComponent portraitB;
    public TachiComponent portraitC;
    public TachiComponent portraitD;
    public TachiComponent portraitE;

    private List<TachiComponent> portraits;

    public void Awake() {
        portraits = new List<TachiComponent>();
        portraits.Add(portraitA);
        portraits.Add(portraitB);
        portraits.Add(portraitC);
        portraits.Add(portraitD);
        portraits.Add(portraitE);
    }

    public TachiComponent GetPortraitBySlot(string slotLetter) {
        switch (slotLetter.ToLower()) {
            case "a": return portraitA;
            case "b": return portraitB;
            case "c": return portraitC;
            case "d": return portraitD;
            case "e": return portraitE;
            default:
                Assert.IsTrue(false, "Bad slot letter: " + slotLetter);
                return null;
        }
    }

    public TachiComponent GetPortraitByChara(CharaData chara) {
        foreach (TachiComponent portrait in portraits) {
            if (portrait.ContainsChara(chara)) {
                return portrait;
            }
        }
        return null;
    }

    public bool AnyVisible() {
        foreach (TachiComponent portrait in portraits) {
            if (portrait.gameObject.activeSelf) {
                return true;
            }
        }
        return false;
    }

    public IEnumerator FadeOutAll(FadeData fade) {
        List<IEnumerator> fadeOuts = new List<IEnumerator>();
        foreach (TachiComponent portrait in portraits) {
            fadeOuts.Add(portrait.FadeOut(fade));
        }
        yield return StartCoroutine(CoUtils.RunParallel(fadeOuts.ToArray(), this));
    }

    public void HideAll() {
        foreach (TachiComponent portrait in portraits) {
            portrait.gameObject.SetActive(false);
        }
    }

    public void PopulateMemory(ScreenMemory memory) {
        memory.portraitA = portraitA.ToMemory();
        memory.portraitB = portraitB.ToMemory();
        memory.portraitC = portraitC.ToMemory();
        memory.portraitD = portraitD.ToMemory();
        memory.portraitE = portraitE.ToMemory();
    }

    public void PopulateFromMemory(ScreenMemory memory) {
        portraitA.PopulateFromMemory(memory.portraitA);
        portraitB.PopulateFromMemory(memory.portraitB);
        portraitC.PopulateFromMemory(memory.portraitC);
        portraitD.PopulateFromMemory(memory.portraitD);
        portraitE.PopulateFromMemory(memory.portraitE);
    }
}
