using UnityEngine;
using System.Collections;

public class WalkRouteTargeter : Targeter {

    private WalkRouteTargeterParams data;

    public class WalkRouteTargeterParams : TargeterParams {
        public override Targeter Instantiate() {
            return new WalkRouteTargeter(this);
        }
    }

    public WalkRouteTargeter(WalkRouteTargeterParams data) {
        this.data = data;
    }

    public override IEnumerator AcquireTargets(Result<bool> result) {
        throw new System.NotImplementedException();
    }
}
