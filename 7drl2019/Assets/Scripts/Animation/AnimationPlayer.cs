using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LuaContext))]
[DisallowMultipleComponent]
public class AnimationPlayer : MonoBehaviour {
    
    public AnimationTarget target;
    public LuaAnimation anim;

    public bool isPlayingAnimation { get; private set; }

    public virtual void EditorReset() {
        isPlayingAnimation = false;
    }

    public virtual IEnumerator PlayAnimationRoutine(LuaContext context = null) {
        while (isPlayingAnimation) {
            yield return null;
        }
        if (context == null) {
            context = GetComponent<LuaContext>();
        }
        isPlayingAnimation = true;
        LuaScript script = anim.ToScript(context);
        context.SetGlobal("target", target);
        yield return script.RunRoutine();
        isPlayingAnimation = false;
    }

    public IEnumerator PlayAnimationRoutine(LuaAnimation anim, LuaContext context = null) {
        this.anim = anim;
        yield return PlayAnimationRoutine(context);
    }
}
