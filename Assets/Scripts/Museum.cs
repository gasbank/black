using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class Museum : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void GoToLobby()
    {
        SceneManager.LoadScene("Main");
    }
}