using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Cursor")]
    [SerializeField] private bool lockCursorDuringGameplay = false;

    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        ResumeTimeOnly();
    }

    private void Update()
    {
        if (ReadPausePressed())
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        ResumeTimeOnly();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ResumeTimeOnly()
    {
        Time.timeScale = 1f;
        Cursor.lockState = lockCursorDuringGameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursorDuringGameplay;
    }

    private bool ReadPausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}