using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Object {

    private GameObject prefab;
    private List<GameObject> unusedInstances;
    private List<GameObject> allInstances;

    public ObjectPool(GameObject prefab) {
        this.prefab = prefab;

        unusedInstances = new List<GameObject>();
        allInstances = new List<GameObject>();
    }

    public void OnDestroy() {
        foreach (GameObject instance in unusedInstances) {
            Destroy(instance);
        }
    }

    public GameObject GetInstance() {
        GameObject instance;
        if (unusedInstances.Count > 0) {
            instance = unusedInstances[0];
            unusedInstances.RemoveAt(0);
            return instance;
        } else {
            instance = Instantiate(prefab);
            allInstances.Add(instance);
        }
        return instance;
    }

    public void FreeInstance(GameObject instance) {
        Debug.Assert(allInstances.Contains(instance));
        instance.transform.parent = null;
        unusedInstances.Add(instance);
    }
}
