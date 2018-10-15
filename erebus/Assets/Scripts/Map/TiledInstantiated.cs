using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Anything that comes from Tiled2Unity.
 */
public interface ITiledInstantiated {

    void Populate(IDictionary<string, string> properties);

}
