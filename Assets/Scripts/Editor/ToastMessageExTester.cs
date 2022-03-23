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
            message.PlayGoodAnim("업적달성: 10콤보");
        }
        else if (GUILayout.Button("Warning"))
        {
            message.PlayWarnAnim("다른 색을 칠하셨습니다.");
        }
    }
}
