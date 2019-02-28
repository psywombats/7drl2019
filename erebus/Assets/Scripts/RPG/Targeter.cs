using System.Collections;
using UnityEngine;

/**
 * Abstract to cover single-empty-square-at-range vs one-direction. Serialized props on each
 * instance are stuff like range, radius... mostly simple.
 **/
public abstract class Targeter {

    /**
     * The targeter is responsible for doing whatever it needs to do to be ready to be consumed by a
     * warhead. The result is true if the targeting succeeded, false if the player canceled it. If
     * the result isn't canceled, there targeter instance can be passed to a warhead and consumed.
     */
    public abstract IEnumerator AcquireTargets(Result<bool> result);

    /**
     * All properties common to parameters for targeters. Subclasses should subclass this usually.
     */
    public abstract class TargeterParams : ScriptableObject {

        /**
         * Return an targetor of the appropriate subclass.
         */
        public abstract Targeter Instantiate();
    }
}
