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

        // 예를 들어 현재 보여주려는 property가 nested 구조인 경우
        // (예: achievementGathered.maxSushiLevel)
        // 'property.serializedObject.targetObject'의 'achievementGathered' 필드 정보를 찾고,
        // 필드 정보로 achievementGathered의 값을 가져와서 targetObject로 쓴다.
        // nested 구조가 아니면 'property.serializedObject.targetObject'를 그대로 targetObject로 쓰면 된다.
        // 지금은 1단계 nested까지만 하드코딩으로 지원.
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