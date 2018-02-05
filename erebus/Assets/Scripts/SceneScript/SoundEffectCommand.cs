using System;
using System.Collections;
using UnityEngine;

public class SoundEffectCommand : SceneCommand {

    private string soundTag;

    public SoundEffectCommand(string soundTag) {
        this.soundTag = soundTag;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        SoundPlayer soundPlayer = player.GetSound();
        soundPlayer.PlaySound(soundTag);
        yield return null;
    }
}
