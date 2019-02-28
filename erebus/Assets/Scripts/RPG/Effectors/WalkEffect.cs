using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ...You walk to the selected location.
 */
public class WalkEffect : Effector {

    private WalkEffectParams data;

    public class WalkEffectParams : EffectorParams {
        public override Effector Instantiate() {
            return new WalkEffect(this);
        }
    }

    public WalkEffect(WalkEffectParams data) {
        this.data = data;
    }

    public override IEnumerator Execute(Targeter targeterInstance) {
        throw new System.NotImplementedException();
    }
}
