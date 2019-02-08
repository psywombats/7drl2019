using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;

public abstract class PolymorphicFieldEditor : Editor {

    private Dictionary<string, Type> cachedSubclasses;

    protected T DrawSelector<T>(T obj) where T : ScriptableObject {
        string[] names = GetSubclasses().Keys.ToArray();

        int index = 0;
        if (obj != null) {
            index = GetSubclasses().Values.ToList().IndexOf(obj.GetType());
        }
        int selectedIndex = EditorGUILayout.Popup(GetBaseType().Name + " Type", index, names);

        if (selectedIndex != index) {
            if (obj != null) {
                AssetDatabase.DeleteAsset(PathForTarget());
                return null;
            }
            if (selectedIndex != 0) {
                Type type = GetSubclasses().Values.ToArray()[selectedIndex];
                T instance = (T) CreateInstance(type);
                string name = target.name + "_warhead";
                AssetDatabase.CreateAsset(instance, PathForTarget());
                return instance;
            }
        }
        return obj;
    }

    protected Dictionary<string, Type> GetSubclasses() {
        if (cachedSubclasses == null) {
            cachedSubclasses = new Dictionary<string, Type> {
                { "(none)", null }
            };
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in ass.GetTypes()) {
                    if (GetBaseType().IsAssignableFrom(type) && !type.IsAbstract) {
                        cachedSubclasses.Add(type.Name, type);
                    }
                }
            }
        }
        return cachedSubclasses;
    }

    protected abstract Type GetBaseType();

    protected abstract string PathForTarget();
}
