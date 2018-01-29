using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
        foreach (Transform child in prefab.transform) {
            if (child.gameObject.GetComponent<Layer3D>() != null) {
                child.gameObject.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                Vector3 oldPosition = child.gameObject.transform.position;
                child.gameObject.transform.position = new Vector3(oldPosition.x, child.GetComponent<Layer3D>().Z, oldPosition.z);
            }
        }
    }

    protected void Populate<T>(GameObject prefab, IDictionary<string, string> props) where T : TiledInstantiated {
        if (prefab.GetComponent<T>() == null) {
            prefab.AddComponent<T>();
        }
        T component = prefab.GetComponent<T>();
        component.Populate(props);
    }
}
