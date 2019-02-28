using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class PolymorphicFieldUtility {

    private Type baseType;
    private string pathForTarget;
    private Dictionary<string, Type> cachedSubclasses;

    public PolymorphicFieldUtility(Type baseType, string pathForTarget) {
        this.baseType = baseType;
        this.pathForTarget = pathForTarget;
    }

    public T DrawSelector<T>(T obj) where T : ScriptableObject {
        string[] names = GetSubclasses().Keys.ToArray();

        int index = 0;
        if (obj != null) {
            index = GetSubclasses().Values.ToList().IndexOf(obj.GetType());
        }
        int selectedIndex = EditorGUILayout.Popup(baseType.Name + " Type", index, names);

        if (selectedIndex != index) {
            if (obj != null) {
                AssetDatabase.DeleteAsset(pathForTarget);
                return null;
            }
            if (selectedIndex != 0) {
                Type type = GetSubclasses().Values.ToArray()[selectedIndex];
                T instance = (T)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, pathForTarget);
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
                    if (baseType.IsAssignableFrom(type) && !type.IsAbstract) {
                        cachedSubclasses.Add(type.Name, type);
                    }
                }
            }
        }
        return cachedSubclasses;
    }
}
