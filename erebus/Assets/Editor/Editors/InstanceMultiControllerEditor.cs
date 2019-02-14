using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InstanceMultiController))]
public class InstanceMultiControllerEditor : Editor {

    SerializedProperty autoApply;

    public void OnEnable() {
        autoApply = serializedObject.FindProperty("AutoApply");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(autoApply);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Apply")) {
            ((InstanceMultiController)target).CopyAllComponentsInScene();
        }
    }
}
