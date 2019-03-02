﻿using UnityEngine;
using System.Collections.Generic;

public abstract class GenericIndex<T> : ScriptableObject where T : GenericDataObject {

    public List<T> dataObjects;

    private Dictionary<string, T> tagToDataObject;

    public void OnEnable() {
        if (dataObjects == null) {
            return;
        }
        tagToDataObject = new Dictionary<string, T>();
        foreach (T dataObject in dataObjects) {
            tagToDataObject[dataObject.tag] = dataObject;
        }
    }

    public T GetData(string tag) {
        if (!tagToDataObject.ContainsKey(tag.ToLower())) {
            Debug.LogError("Index " + this.GetType().Name + " does not contain key\"" + tag + "\"");
            return null;
        }
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
