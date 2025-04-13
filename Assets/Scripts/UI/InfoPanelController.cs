using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Transform unitListParent;
    [SerializeField] private GameObject unitIconPrefab;
    [SerializeField] private TextMeshProUGUI spawnStatusText;    
    [SerializeField] private Button toggleModeButton;
    [SerializeField] private TextMeshProUGUI toggleModeButtonText;
    [SerializeField] private Image toggleModeButtonIcon;

    private BaseBuilding selectedBuilding;

    private void Start()
    {
        ClearSelectionUI();
        
        if (spawnStatusText != null)
        {
            spawnStatusText.text = "";
            spawnStatusText.gameObject.SetActive(false);
        }
        
        if (toggleModeButton != null)
        {
            toggleModeButton.onClick.AddListener(ToggleBuildingMode);
            toggleModeButton.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (UIEventDispatcher.Instance != null)
        {
            UIEventDispatcher.Instance.RegisterBuildingClickListener(OnBuildingSelected);
            UIEventDispatcher.Instance.OnBuildingModeChanged += OnBuildingModeChanged;
        }
        
        if (selectedBuilding == null)
            ClearSelectionUI();
    }

    private void OnDisable()
    {
        if (UIEventDispatcher.Instance != null)
        {
            UIEventDispatcher.Instance.UnregisterBuildingClickListener(OnBuildingSelected);
            UIEventDispatcher.Instance.OnBuildingModeChanged -= OnBuildingModeChanged;
        }
    }

    private void Update()
    {
        if (selectedBuilding != null)
        {
            UpdateHPText();
            
            if (selectedBuilding.currentHP <= 0)
            {
                selectedBuilding = null;
                ClearSelectionUI();
            }
        }
    }

    private void UpdateHPText()
    {
        if (selectedBuilding != null)
            hpText.text = $"HP: {selectedBuilding.currentHP} / {selectedBuilding.maxHP}";
    }

    public void OnBuildingSelected(BuildingData data, BaseBuilding instance)
    {
        if (instance != null && instance.currentHP <= 0)
        {
            ClearSelectionUI();
            return;
        }
        
        selectedBuilding = instance;
        
        if (instance != null && data != null)
            UpdateBuildingUI(data, instance);
        else
            ClearSelectionUI();
    }

    private void UpdateBuildingUI(BuildingData data, BaseBuilding instance)
    {
        iconImage.gameObject.SetActive(true);
        iconImage.sprite = data.icon;
        nameText.text = data.buildingName;
        UpdateHPText();

        ClearUnitList();

        if (data.buildingName == "Barrack")
            PopulateBarrackUnits(instance as Barrack, data);
            
        bool hasBacksideSprite = (instance.backsideSprite != null);
        if (toggleModeButton != null)
        {
            toggleModeButton.gameObject.SetActive(hasBacksideSprite);
            
            if (toggleModeButtonText != null)
            {
                toggleModeButtonText.text = instance.IsBacksideMode ? "Switch to Front" : "Switch to Back";
            }
        }
    }

    private void ClearSelectionUI()
    {
        iconImage.gameObject.SetActive(false);
        nameText.text = "";
        hpText.text = "";
        ClearUnitList();
        
        if (spawnStatusText != null)
        {
            spawnStatusText.text = "";
            spawnStatusText.gameObject.SetActive(false);
        }
        
        if (toggleModeButton != null)
        {
            toggleModeButton.gameObject.SetActive(false);
        }
    }

    private void ClearUnitList()
    {
        foreach (Transform child in unitListParent)
            Destroy(child.gameObject);
    }

    private void PopulateBarrackUnits(Barrack barrack, BuildingData data)
    {
        if (barrack == null) return;
        foreach (var unit in data.trainableUnits)
        {
            GameObject icon = Instantiate(unitIconPrefab, unitListParent);
            Image unitImage = icon.transform.Find("UnitImage").GetComponent<Image>();
            if (unitImage != null) unitImage.sprite = unit.icon;

            TextMeshProUGUI unitNameText = icon.transform.Find("UnitNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI unitDamageText = icon.transform.Find("UnitDamageText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI unitHpText = icon.transform.Find("UnitHpText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI unitSpeedText = icon.transform.Find("UnitSpeedText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI unitAttackSpeedText = icon.transform.Find("UnitAttackSpeedText").GetComponent<TextMeshProUGUI>();

            if (unitNameText != null) unitNameText.text = unit.unitName;
            if (unitDamageText != null) unitDamageText.text = $"{unit.damage}";
            if (unitHpText != null) unitHpText.text = $"{unit.hp}";
            if (unitSpeedText != null) unitSpeedText.text = $"{unit.moveSpeed}";
            if (unitAttackSpeedText != null) unitAttackSpeedText.text = $"{unit.attackCooldown}";

            Button btn = icon.transform.Find("UnitButton").GetComponent<Button>();
            if (btn != null)
            {
                UnitData capturedUnit = unit;
                btn.onClick.AddListener(() => SpawnUnitFromBarrack(barrack, capturedUnit));
            }
        }
    }

    private void SpawnUnitFromBarrack(Barrack barrack, UnitData unitData)
    {
        Vector3 spawnPos = barrack.ActiveSpawnPoint.position;
        
        if (IsSpawnAreaClear(spawnPos, unitData))
        {
            UnitFactory.Instance.SpawnUnit(unitData, spawnPos);
        }
        else
        {
            if (spawnStatusText != null)
            {
                spawnStatusText.gameObject.SetActive(true);
                spawnStatusText.text = "Cannot spawn unit, area blocked. maybe you can try another way";
                
                StartCoroutine(HideStatusTextAfterDelay(2f));
            }
            }
    }
    
    private bool IsSpawnAreaClear(Vector3 position, UnitData unitData)
    {
        float checkRadius = 0.1f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius);
        
        foreach (Collider2D collider in colliders)
        {
            BaseBuilding building = collider.GetComponent<BaseBuilding>();
            if (building != null && building != selectedBuilding)
            {
                return false; 
            }
        }
        
        return true;
    }
    
    public void ToggleBuildingMode()
    {
        if (selectedBuilding == null) return;
        
        selectedBuilding.ToggleBuildingMode();
    }
    
    private void OnBuildingModeChanged(BaseBuilding building, bool isBacksideMode)
    {
        if (building != selectedBuilding) return;
        
        if (toggleModeButtonText != null)
        {
            toggleModeButtonText.text = isBacksideMode ? "Switch to Front" : "Switch to Back";
        }
    }
    
    private IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (spawnStatusText != null)
        {
            spawnStatusText.gameObject.SetActive(false);
        }
    }
}