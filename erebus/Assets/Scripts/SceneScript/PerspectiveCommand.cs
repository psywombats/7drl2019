using UnityEngine;
using System.Collections;
using System;

public class PerspectiveCommand : SceneCommand {

    private SpriteEffectComponent effect;

    private string targetCharaKey;
    private string newBackgroundTag;
    private string text;

    public PerspectiveCommand(string targetCharaKey, string newBackgroundTag, string text) {
        this.targetCharaKey = targetCharaKey;
        this.newBackgroundTag = newBackgroundTag;
        this.text = text;
    }

    public override IEnumerator PerformAction() {
        ScenePlayer player = Global.Instance().ScenePlayer;
        effect = player.GetEffect();

        yield return player.StartCoroutine(player.paragraphBox.Deactivate(player));
        yield return player.StartCoroutine(player.textbox.Deactivate(player));

        yield return player.ExecuteTransition("whiteout_in", () => {
            player.StartCoroutine(effect.StartWhiteoutRoutine(0.0f));
        });
        yield return new WaitForSeconds(1.5f);

        TachiComponent tachi = player.portraits.GetPortraitBySlot("D");
        yield return player.StartCoroutine(CoUtils.RunParallel(new[] {
            tachi.FadeCharaIn(targetCharaKey, Global.Instance().Database.Fades.GetData("whiteout_chara_in")),
            effect.FadeLetterboxesIn()
        }, player));
        yield return new WaitForSeconds(0.8f);

        effect.letterboxText.GetComponent<FadingUIComponent>().FadeSeconds = 0.0f;
        yield return player.StartCoroutine(effect.letterboxText.Activate(player));
        yield return player.StartCoroutine(effect.letterboxText.ShowText(player, text, true));

        effect.letterboxText.GetComponent<FadingUIComponent>().FadeSeconds = 0.3f;
        player.StartCoroutine(effect.letterboxText.Deactivate(player));

        yield return player.ExecuteTransition("whiteout_out", () => {
            effect.HideLetterboxes();
            player.StartCoroutine(effect.StopWhiteoutRoutine(0.0f));
            if (newBackgroundTag != null) {
                player.background.SetBackground(newBackgroundTag);
            }
        });
    }
}
