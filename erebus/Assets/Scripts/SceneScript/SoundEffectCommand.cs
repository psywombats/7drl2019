using System;
using System.Collections;
using UnityEngine;

public class SoundEffectCommand : SceneCommand {

    private string soundTag;

    public SoundEffectCommand(string soundTag) {
        this.soundTag = soundTag;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        Global.Instance().Audio.PlaySFX(soundTag);
        yield return null;
    }
}
