using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform productionMenuContent;
    [SerializeField] private GameObject buildingItemPrefab;
    [SerializeField] private BuildingDatabase buildingDatabase;
    [SerializeField] private GameObject productionMenuPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject gotoMenuPanel;
    [SerializeField] private GameObject productionMenuToggleButton;
    [SerializeField] private GameObject infoPanelToggleButton;
    [SerializeField] private GameObject gotoMenuToggleButton;

    [Header("Infinite Scroll View")]
    [SerializeField] private UIObjectPool buildingItemPool;
    [SerializeField] private CircularScrollView circularScrollView;

    [Header("Sound Settings")]
    [SerializeField] private Button soundButton;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioListener audioListener;

    private bool isProductionMenuVisible;
    private bool isInfoPanelVisible;
    private bool isGotoMenuVisible;
    
    private const string SOUND_ENABLED_KEY = "SoundEnabled";
    private bool isSoundOn;

    void Awake()
    {
        isProductionMenuVisible = productionMenuPanel.activeSelf;
        isInfoPanelVisible = infoPanel.activeSelf;
    }

    void Start()
    {

        if (productionMenuToggleButton != null)
        {
            productionMenuToggleButton.GetComponent<Button>().onClick.AddListener(ToggleProductionMenu);
        }

        if (infoPanelToggleButton != null)
        {
            infoPanelToggleButton.GetComponent<Button>().onClick.AddListener(ToggleInfoPanel);
        }

        if (gotoMenuToggleButton != null)
        {
            gotoMenuToggleButton.GetComponent<Button>().onClick.AddListener(ToggleGotoMenu);
        }

        if (soundButton != null)
        {
            soundButton.onClick.AddListener(ToggleSound);
        }

        InitializeSoundSetting();
    }

    public void ToggleProductionMenu()
    {
        isProductionMenuVisible = !isProductionMenuVisible;
        productionMenuPanel.SetActive(isProductionMenuVisible);
        UIEventDispatcher.Instance.ProductionMenuVisibilityChanged(isProductionMenuVisible);
    }

    public void ToggleInfoPanel()
    {
        isInfoPanelVisible = !isInfoPanelVisible;
        infoPanel.SetActive(isInfoPanelVisible);
        UIEventDispatcher.Instance.InfoPanelVisibilityChanged(isInfoPanelVisible);
    }

    public void ToggleGotoMenu()
    {
        isGotoMenuVisible = !isGotoMenuVisible;
        gotoMenuPanel.SetActive(isGotoMenuVisible);
        UIEventDispatcher.Instance.GotoMenuVisibilityChanged(isGotoMenuVisible);
    }

    public void ShowProductionMenu(bool show)
    {
        isProductionMenuVisible = show;
        productionMenuPanel.SetActive(show);
    }

    public void ShowInfoPanel(bool show)
    {
        isInfoPanelVisible = show;
        infoPanel.SetActive(show);
    }
    
    public void RefreshBuildingList()
    {
        if (circularScrollView != null)
        {
            circularScrollView.ReloadData();
        }
    }

    private void InitializeSoundSetting()
    {
        isSoundOn = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
        ApplySoundSetting(isSoundOn);
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