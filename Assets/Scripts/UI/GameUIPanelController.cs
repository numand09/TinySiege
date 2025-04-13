using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIPanelController : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button restartButton;

    [Header("Scene References")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMainMenu);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartCurrentLevel);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}