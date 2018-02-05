using UnityEngine;
using System.Collections;
using System;

public class BGMCommand : SceneCommand {

    private string tag;

	public BGMCommand(string tag) {
        this.tag = tag;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        AudioManager bgm = Global.Instance().Audio;
        bgm.StartCoroutine(bgm.CrossfadeRoutine(tag));
        yield return null;
    }
}
