using UnityEngine;
using System.Collections;
using System;

public abstract class SceneCommand {

    public abstract IEnumerator PerformAction();

    // focus can be lost when the menu or something pops up

    public virtual void OnFocusGained() {
        // nothing
    }

    public virtual void OnFocusLost() {
        // nothing
    }
}
