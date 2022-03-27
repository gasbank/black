using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToastMessageEx))]
public class ToastMessageExTester : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Good"))
        {
            ToastMessageEx.instance.PlayGoodAnim("업적달성: 10콤보");
        }
        else if (GUILayout.Button("Warning"))
        {
            ToastMessageEx.instance.PlayWarnAnim("색을 다시 확인해주세요");
        }
    }
}
