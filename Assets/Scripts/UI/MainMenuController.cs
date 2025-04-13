using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button soundButton;

    [Header("Sound Button Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioListener audioListener;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private const string SOUND_ENABLED_KEY = "SoundEnabled";
    private bool isSoundOn;

    private void Start()
    {
        SetupButtons();
        InitializeSoundSetting();
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
    }

    private void InitializeSoundSetting()
    {
        isSoundOn = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
        ApplySoundSetting(isSoundOn);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;

        PlayerPrefs.SetInt(SOUND_ENABLED_KEY, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundSetting(isSoundOn);
    }

     private void ApplySoundSetting(bool enabled)
{
    if (audioSource != null)
        audioSource.enabled = enabled;
    if (audioListener != null)
        audioListener.enabled = enabled;
    
    if (soundButton != null)
    {
        Image buttonImage = soundButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            if ((enabled && soundOnSprite != null) || (!enabled && soundOffSprite != null))
            {
                buttonImage.sprite = enabled ? soundOnSprite : soundOffSprite;
            }
        }
    }
}
    private void OnEnable()
{
    InitializeSoundSetting();
}
}
