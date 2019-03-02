// modified from https://gist.github.com/tomkail/ba4136e6aa990f4dc94e0d39ec6a058c

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AutoExpandingScriptableObject), true)]
//[CustomPropertyDrawer(typeof(UnityEngine.Tilemaps.Tile), true)]
public class AutoExpandingScriptableDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float totalHeight = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded) {
            var data = property.objectReferenceValue as ScriptableObject;
            if (data == null) return EditorGUIUtility.singleLineHeight;
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true)) {
                do {
                    if (prop.name == "m_Script") continue;
                    var subProp = serializedObject.FindProperty(prop.name);
                    float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
                    totalHeight += height;
                }
                while (prop.NextVisible(false));
            }
            // Add a tiny bit of height if open for the background
            totalHeight += EditorGUIUtility.standardVerticalSpacing;
        }
        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.objectReferenceValue != null) {
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(new Rect(
                position.x, 
                position.y, 
                EditorGUIUtility.labelWidth, 
                EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
            EditorGUI.PropertyField(new Rect(
                EditorGUIUtility.labelWidth + 14,
                position.y, 
                position.width - EditorGUIUtility.labelWidth, 
                EditorGUIUtility.singleLineHeight), property, GUIContent.none, true);
            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
            if (property.objectReferenceValue == null) EditorGUIUtility.ExitGUI();

            if (property.isExpanded) {
                // Draw a background that shows us clearly which fields are part of the ScriptableObject
                GUI.Box(new Rect(
                    0, 
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, 
                    Screen.width, 
                    position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

                EditorGUI.indentLevel++;
                var data = (ScriptableObject)property.objectReferenceValue;
                SerializedObject serializedObject = new SerializedObject(data);

                // Iterate over all the values and draw them
                SerializedProperty prop = serializedObject.GetIterator();
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (prop.NextVisible(true)) {
                    do {
                        // Don't bother drawing the class file
                        if (prop.name == "m_Script") continue;
                        float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
                        y += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (prop.NextVisible(false));
                }
                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        } else {
            base.OnGUI(position, property, label);
        }
    }
}