using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Anything that comes from Tiled2Unity.
 */
public abstract class TiledInstantiated : MonoBehaviour {

    public abstract void Populate(IDictionary<string, string> properties);

}
