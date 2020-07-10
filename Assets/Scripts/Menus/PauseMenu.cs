using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    bool settingsOn = false;

    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    void Update()
    {
        // PAUSE MENU
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) {
                Resume();
            }
            else
            {
                Pause();
            }
        }   
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        // TODO : create an AudioManager and change the pitch of the audio when the game is paused
    }

    public void Resume()
    {
        if (settingsOn) // if Settings are up, go back to pause menu
        {
            settingsMenuUI.SetActive(false);
            pauseMenuUI.SetActive(true);
            settingsOn = false;
        }
        else // exit pause menu
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
        }
    }

    public void LoadSettingsMenu()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        settingsOn = true;
    }

    public void LoadMainMenu()
    {
        // TODO : create a main Menu
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Going to Main menu (missing)");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}
