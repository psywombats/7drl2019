﻿using UnityEngine;
using System.Collections;
using System;

public class IncrementCommand : SceneCommand {

    private string variableName;
    private int delta;

    public IncrementCommand(string variableName, int delta) {
        this.variableName = variableName;
        this.delta = delta;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        if (delta > 0) {
            Global.Instance().memory.IncrementVariable(variableName);
        } else {
            Global.Instance().memory.DecrementVariable(variableName);
        }
        yield return null;
    }
}
