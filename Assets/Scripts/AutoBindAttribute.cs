using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class AutoBindAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class AutoBindThisAttribute : Attribute
{
}

public static class AutoBindUtil
{
#if UNITY_EDITOR
    public static void BindAll<T>(T comp) where T : MonoBehaviour
    {
        foreach (var fieldInfo in typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(AutoBindAttribute), false);
            foreach (var attr in attributes)
            {
                var niceName = ObjectNames.NicifyVariableName(fieldInfo.Name);
                //ConDebug.Log($"{fieldInfo.Name} --> {niceName}");
                var targetCompTransform = comp.transform.FindDeepChild(niceName);
                if (SetFieldValue(comp, fieldInfo, targetCompTransform) == false)
                    Debug.LogError($"Cannot find auto bind target: {niceName}", comp.gameObject);
            }

            var thisAttributes = fieldInfo.GetCustomAttributes(typeof(AutoBindThisAttribute), false);
            foreach (var attr in thisAttributes)
                if (SetFieldValue(comp, fieldInfo, comp.transform) == false)
                    Debug.LogError($"Cannot find auto bind this target: {comp.gameObject.name}", comp.gameObject);
        }
    }

    static bool SetFieldValue<T>(T comp, FieldInfo fieldInfo, Transform targetCompTransform) where T : MonoBehaviour
    {
        if (targetCompTransform != null)
        {
            if (fieldInfo.FieldType == typeof(GameObject))
                fieldInfo.SetValue(comp, targetCompTransform.gameObject);
            else
                fieldInfo.SetValue(comp, targetCompTransform.GetComponent(fieldInfo.FieldType));
            return true;
        }

        return false;
    }
#endif
}