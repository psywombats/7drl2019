using UnityEngine;
using System.Collections.Generic;

public abstract class GenericIndex<T> : ScriptableObject where T : GenericDataObject {

    public T[] dataObjects;

    private Dictionary<string, T> tagToDataObject;

    public void OnEnable() {
        tagToDataObject = new Dictionary<string, T>();
        foreach (T dataObject in dataObjects) {
            tagToDataObject[dataObject.tag] = dataObject;
        }
    }

    public T GetData(string tag) {
        return tagToDataObject[tag.ToLower()];
    }

    public T GetDataOrNull(string tag) {
        if (tagToDataObject.ContainsKey(tag.ToLower())) {
            return GetData(tag);
        } else {
            return null;
        }
    }
}
