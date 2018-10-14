using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(StatSet))]
[CanEditMultipleObjects]
public class StatSetEditor : Editor {

    public override void OnInspectorGUI() {
        StatSet stats = (StatSet)target;

        //foreach (AdditiveStat stat in Enum.GetValues(typeof(AdditiveStat))) {
        //    EditorGUILayout.FloatField()
        //}
    }
}
