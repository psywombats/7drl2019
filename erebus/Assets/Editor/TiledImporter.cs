using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;
using System;

[Tiled2Unity.CustomTiledImporter]
public class TiledImporter : ICustomTiledImporter {

    // Property handling and custom object assignment
    virtual public void HandleCustomProperties(UnityEngine.GameObject prefab, IDictionary<string, string> props) {
        if (prefab.GetComponent<TiledMap>() != null) {
            Populate<Map>(prefab, props);
        } else if (prefab.GetComponent<RuntimeTmxObject>() != null) {
            Populate<MapEvent>(prefab, props);
        } else if (prefab.GetComponent<Layer>() != null) {
            if (props.ContainsKey("3d")) {
                Populate<Layer3D>(prefab, props);
            }
        }
    }

    public void CustomizePrefab(GameObject prefab) {
        // nothing for now
    }

    protected void Populate<T>(GameObject prefab, IDictionary<string, string> props) where T : TiledInstantiated {
        if (prefab.GetComponent<T>() == null) {
            prefab.AddComponent<T>();
        }
        T component = prefab.GetComponent<T>();
        component.Populate(props);
    }
}
