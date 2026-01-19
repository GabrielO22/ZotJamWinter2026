using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;



public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject endScreen;

    private bool isPaused = false;
    private bool hasEnded = false;

    void Start()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (endScreen != null) endScreen.SetActive(false);
    }

    void Update()
    {
        if (!hasEnded && Input.GetKeyDown(KeyCode.Escape))
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
    }
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

    public void showendScreen()
    {
        if (hasEnded) return;

        hasEnded = true;

        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(false);

        if (endScreen != null) endScreen.SetActive(true);
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gotoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
    