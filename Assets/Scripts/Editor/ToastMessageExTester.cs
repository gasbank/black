using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToastMessageEx))]
public class ToastMessageExTester : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ToastMessageEx message = (ToastMessageEx)target;
        if (GUILayout.Button("Good"))
        {
            message.PlayGoodAnim();
        }
        else if (GUILayout.Button("Warning"))
        {
            message.PlayWarnAnim();
        }
    }
}
