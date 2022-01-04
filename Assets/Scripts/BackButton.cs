using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void GoToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}