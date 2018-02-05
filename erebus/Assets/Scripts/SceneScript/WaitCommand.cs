using System;
using System.Collections;
using UnityEngine;

public class WaitCommand : SceneCommand {

    private float durationSeconds;

    public WaitCommand(string durationSecondsString) {
        this.durationSeconds = (float) Convert.ToDouble(durationSeconds);
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        yield return new WaitForSeconds(durationSeconds);
    }
}
