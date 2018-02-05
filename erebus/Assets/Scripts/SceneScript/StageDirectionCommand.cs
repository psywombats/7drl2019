using UnityEngine;
using System.Collections;

public abstract class StageDirectionCommand : SceneCommand {

    protected bool synchronous;

    public void SetSynchronous() {
        synchronous = true;
    }
}
