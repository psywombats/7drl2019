using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StatSet))]
public class StatSetDrawer : PropertyDrawer {

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label) {
        SerializedProperty serialDictionary = property.FindPropertyRelative("serializedStats");
        SerializedProperty keys = serialDictionary.FindPropertyRelative("keys");
        SerializedProperty values = serialDictionary.FindPropertyRelative("values");

        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textArea);
        fieldStyle.fixedWidth = 80;

        EditorGUI.LabelField(
            new Rect(pos.x, 6 + pos.y, pos.width, pos.height),
            "Stats");

        for (int i = 0; i < keys.arraySize; i += 1) {
            SerializedProperty serializedValue = values.GetArrayElementAtIndex(i);
            Stat stat = Stat.Get(keys.GetArrayElementAtIndex(i).enumValueIndex);
            float value = serializedValue.floatValue;
            
            EditorGUI.LabelField(
                new Rect(18 + pos.x, 24 + pos.y + i * 18, pos.width, pos.height),
                stat.nameShort + ": ");
            if (stat.useBinaryEditor) {
                serializedValue.floatValue = EditorGUI.Toggle(
                    new Rect(pos.x + 100, 24 + pos.y + i * 18, pos.width, pos.height),
                    (value > 0.0f)) ? 1.0f : 0.0f;
            } else {
                serializedValue.floatValue = EditorGUI.FloatField(
                    new Rect(pos.x + 100, 24 + pos.y + i * 18, pos.width, pos.height),
                    value,
                    fieldStyle);
            }

        }
    }
}