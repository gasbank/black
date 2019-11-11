using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScBigInteger))]
public class ScBigIntegerDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var valueRect = new Rect(position.x, position.y, position.width, position.height);
        var scBigInteger = (ScBigInteger)fieldInfo.GetValue(property.serializedObject.targetObject);
        var newScBigIntegerStr = EditorGUI.TextField(valueRect, scBigInteger);
        fieldInfo.SetValue(property.serializedObject.targetObject, new ScBigInteger(newScBigIntegerStr));

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}