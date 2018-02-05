using UnityEngine;
using System.Collections;
using System;

public class EnterCommand : StageDirectionCommand {

    private const string DefaultFadeOutTag = "fade";
    private const string DefaultFadeInTag = "fade";

    private string charaTag;
    private string slotLetter;
    private string fadeTag;

    // chara name is the name of a character, ie "max"
    // slot letter is the name of the screen slot to position, from A to E, far left to far right
    // fade tag tags a fade, or null for default
    public EnterCommand(string charaTag, string slotLetter, string fadeTag) {
        this.charaTag = charaTag;
        this.slotLetter = slotLetter;
        this.fadeTag = (fadeTag == null) ? DefaultFadeInTag : fadeTag;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        if (synchronous) {
            yield return player.StartCoroutine(ParallelAction(player));
        } else {
            yield return null;
            player.StartCoroutine(ParallelAction(player));
        }
    }

    private IEnumerator ParallelAction(ScenePlayer player) {
        TachiComponent portrait = player.portraits.GetPortraitBySlot(slotLetter);

        // fade out if someone's there already
        if (portrait.gameObject.activeSelf) {
            yield return portrait.FadeOut(player.fades.GetData(DefaultFadeOutTag));
        }

        // fade in the referenced chara
        yield return portrait.FadeCharaIn(charaTag, player.fades.GetData(fadeTag));
    }
}
