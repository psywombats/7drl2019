using UnityEngine;
using System.Collections;
using System;

public class ExitAllCommand : StageDirectionCommand {

    private const string DefaultFadeTag = "fade";

    public bool ClosesTextboxes { get; set; }

    public override IEnumerator PerformAction() {
        ScenePlayer player = Global.Instance().ScenePlayer;
        if (ClosesTextboxes) {
            yield return CoUtils.RunParallel(new[] {
                    player.textbox.Deactivate(player),
                    player.paragraphBox.Deactivate(player)
            }, player);
        }

        if (!player.portraits.AnyVisible()) {
            yield return null;
        } else {
            FadeData fade = Global.Instance().Database.Fades.GetData(DefaultFadeTag);
            if (synchronous) {
                yield return player.StartCoroutine(player.portraits.FadeOutAll(fade));
            } else {
                yield return null;
                player.StartCoroutine(player.portraits.FadeOutAll(fade));
            }

            Global.Instance().Memory.AppendLogItem(new LogItem(""));
        }
    }
}
