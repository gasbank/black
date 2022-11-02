using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToastMessage))]
public class ToastMessageTester : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Good"))
        {
            ToastMessage.instance.PlayGoodAnim("업적달성: 10콤보");
        }
        else if (GUILayout.Button("Warning"))
        {
            ToastMessage.instance.PlayWarnAnim("색을 다시 확인해주세요");
        }
    }
}