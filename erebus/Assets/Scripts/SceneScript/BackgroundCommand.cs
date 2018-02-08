using UnityEngine;
using System.Collections;
using System;

public class BackgroundCommand : SceneCommand {

    private const string DefaultTransitionTag = "fade";

    private string backgroundTag;
    private string transitionTag;

    public BackgroundCommand(string backgroundTag, string transitionTag) {
        this.backgroundTag = backgroundTag;
        this.transitionTag = (transitionTag == null) ? DefaultTransitionTag : transitionTag;
    }

    public override IEnumerator PerformAction() {
        ScenePlayer player = Global.Instance().ScenePlayer;
        yield return player.StartCoroutine(player.paragraphBox.Deactivate(player));
        yield return player.StartCoroutine(player.textbox.Deactivate(player));
        yield return player.StartCoroutine(player.ExecuteTransition(transitionTag, () => {
            player.background.SetBackground(backgroundTag);
        }));
    }
}
