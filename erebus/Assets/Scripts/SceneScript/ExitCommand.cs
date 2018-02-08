using UnityEngine;
using System.Collections;
using System;

public class ExitCommand : StageDirectionCommand {

    private const string DefaultFadeTag = "fade";

    private string charaTag;
    private string fadeTag;

    // chara name is the name of a character, ie "max"
    // fade tag tags a fade, ie "bars", if null will use default
    public ExitCommand(string charaTag, string fadeTag) {
        this.charaTag = charaTag;
        this.fadeTag = fadeTag == null ? DefaultFadeTag : fadeTag;
    }

    public override IEnumerator PerformAction() {
        ScenePlayer player = Global.Instance().ScenePlayer;
        if (synchronous) {
            yield return player.StartCoroutine(ParallelAction(player));
        } else {
            yield return null;
            player.StartCoroutine(ParallelAction(player));
        }
    }

    private IEnumerator ParallelAction(ScenePlayer player) {
        CharaData chara = player.portraits.charas.GetData(charaTag);
        TachiComponent portrait = player.portraits.GetPortraitByChara(chara);

        // fade 'em out!
        if (portrait != null) {
            yield return portrait.FadeOut(player.fades.GetData(fadeTag));
        }
    }
}
