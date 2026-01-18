using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void playGame()
    {
        SceneManager.LoadScene("DebuggingScene");
    }
}
