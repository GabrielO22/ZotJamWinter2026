using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;

    public void onPause()
    {
        if (isPaused)
        {
            resumeGame();
        }
        else
        {
            pauseGame();
        }
    }


    private void pauseGame()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        isPaused = true;
    }

    private void resumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu?.SetActive(false);
        isPaused = false;
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
    