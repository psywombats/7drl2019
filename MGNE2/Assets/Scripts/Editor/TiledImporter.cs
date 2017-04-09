using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiled2Unity;
using System;

[Tiled2Unity.CustomTiledImporter]
public class TiledImporter : ICustomTiledImporter {

    // Property handling and custom object assignment
    public void HandleCustomProperties(UnityEngine.GameObject prefab, IDictionary<string, string> props) {
        if (prefab.GetComponent<TiledMap>() != null) {
            Populate<Map>(prefab, props);
        } else if (prefab.GetComponent<RuntimeTmxObject>() != null) {
            Populate<MapEvent>(prefab, props);
        }
    }

    public void CustomizePrefab(GameObject prefab) {
        // nothing for now
    }

    private void Populate<T>(GameObject prefab, IDictionary<string, string> props) where T : TiledInstantiated {
        prefab.AddComponent<T>();
        T component = prefab.GetComponent<T>();
        component.Populate(props);
    }
}
