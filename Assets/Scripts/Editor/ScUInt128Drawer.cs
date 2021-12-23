using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScUInt128))]
public class ScUInt128Drawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        object targetObject = null;
        string[] propertyPathSplit = property.propertyPath.Split('.');
        if (propertyPathSplit.Length > 2) {
            Debug.LogError("Nested property not supported!");
        } else if (propertyPathSplit.Length == 2) {
            var nestedFieldInfo = property.serializedObject.targetObject.GetType().GetField(propertyPathSplit[0], BindingFlags.NonPublic | BindingFlags.Instance);
            targetObject = nestedFieldInfo.GetValue(property.serializedObject.targetObject);
        } else {
            targetObject = property.serializedObject.targetObject;
        }

        // Calculate rects
        var valueRect = new Rect(position.x, position.y, position.width, position.height);
        var scUint128 = (ScUInt128)fieldInfo.GetValue(targetObject);
        var newScUintStr = EditorGUI.TextField(valueRect, scUint128);
        fieldInfo.SetValue(targetObject, new ScUInt128(newScUintStr));

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}