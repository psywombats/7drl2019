using UnityEngine;
using System.Collections;
using System;

public class GotoCommand : SceneCommand {

    private string sceneName;

    public GotoCommand(string sceneName) {
        this.sceneName = sceneName;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        yield return player.StartCoroutine(player.PlayScriptForScene(sceneName));
    }
}
