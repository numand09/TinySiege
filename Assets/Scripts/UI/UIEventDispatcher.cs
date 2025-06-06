using System;
using UnityEngine;

public class UIEventDispatcher : MonoBehaviour
{
    private static UIEventDispatcher instance;
    
    public static UIEventDispatcher Instance 
    { 
        get => instance;
        private set => instance = value;
    }

    // Events

    public event Action<BuildingData, BaseBuilding> OnBuildingItemClicked;
    public event Action<BaseBuilding, bool> OnBuildingModeChanged;
    private event Action<bool> OnProductionMenuVisibilityChanged;
    private event Action<bool> OnInfoPanelVisibilityChanged;
    private event Action<bool> OnGotoMenuVisibilityChanged;
    public delegate void BuildingModeChangeHandler(BaseBuilding building, bool isBacksideMode);



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BuildingClicked(BuildingData data, BaseBuilding instance)
    {
        OnBuildingItemClicked?.Invoke(data, instance);
    }
    
    public void BuildingModeChanged(BaseBuilding building, bool isBacksideMode)
    {
        OnBuildingModeChanged?.Invoke(building, isBacksideMode);
    }
    
    public void RegisterBuildingClickListener(Action<BuildingData, BaseBuilding> listener)
    {
        OnBuildingItemClicked += listener;
    }
    
    public void UnregisterBuildingClickListener(Action<BuildingData, BaseBuilding> listener)
    {
        OnBuildingItemClicked -= listener;
    }

    public void ProductionMenuVisibilityChanged(bool isVisible)
    {
        OnProductionMenuVisibilityChanged?.Invoke(isVisible);
    }

    public void InfoPanelVisibilityChanged(bool isVisible)
    {
        OnInfoPanelVisibilityChanged?.Invoke(isVisible);
    }
    public void GotoMenuVisibilityChanged(bool isVisible)
    {
        OnGotoMenuVisibilityChanged?.Invoke(isVisible);
    }

    public void RegisterProductionMenuVisibilityListener(Action<bool> listener)
    {
        OnProductionMenuVisibilityChanged += listener;
    }

    public void UnregisterProductionMenuVisibilityListener(Action<bool> listener)
    {
        OnProductionMenuVisibilityChanged -= listener;
    }

    public void RegisterInfoPanelVisibilityListener(Action<bool> listener)
    {
        OnInfoPanelVisibilityChanged += listener;
    }

    public void UnregisterInfoPanelVisibilityListener(Action<bool> listener)
    {
        OnInfoPanelVisibilityChanged -= listener;
    }

}