using System.Collections;
using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour
{
    public string buildingName;
    public Transform healthBar;
    public Vector2Int size;
    public Sprite icon;
    public Sprite destructedSprite;
    public int maxHP;
    public int currentHP;
    public BuildingData buildingData;
    public Vector2Int occupiedGridPosition;
    public Sprite backsideSprite;
    public Transform spawnPoint;
    public Transform spawnPointBackside;
    
    public GameObject canNotPlaceIndicator;  
    public GameObject selectedIndicator; 
    
    protected GameBoardManager board;
    protected SpriteRenderer spriteRenderer;
    protected bool isDestroyed = false;
    protected bool isSelected = false;
    protected Color originalColor;
    protected UIManager uiManager;
    protected bool isBacksideMode = false;
    protected virtual float HealthBarScaleFactor => 1.0f;
    
    public virtual void Initialize()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        board = FindObjectOfType<GameBoardManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        if (canNotPlaceIndicator != null)
            canNotPlaceIndicator.SetActive(false);
            
        if (selectedIndicator != null)
            selectedIndicator.SetActive(false);
    }
    
    public virtual void ToggleBuildingMode()
    {
        isBacksideMode = !isBacksideMode;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isBacksideMode ? backsideSprite : buildingData.prefab.GetComponent<SpriteRenderer>().sprite;
        }
        
        if (UIEventDispatcher.Instance != null)
        {
            UIEventDispatcher.Instance.BuildingModeChanged(this, isBacksideMode);
        }
    }
    
    public Transform ActiveSpawnPoint => isBacksideMode ? spawnPointBackside : spawnPoint;
    
    public bool IsBacksideMode => isBacksideMode;
    
    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0); 
        UpdateHealthBar();
        if (currentHP <= 0)
        {
            isDestroyed = true;
            if (spriteRenderer != null && destructedSprite != null)
            {
                spriteRenderer.sprite = destructedSprite;
            }
            StartCoroutine(WaitAndDestroy());
        }
    }
    
    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(2f);
        FreeAreaOnBoard();
        BuildingPool.Instance.ReturnToPool(gameObject);
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
        
        if (selected)
        {
            if (uiManager != null)
                uiManager.ShowInfoPanel(this);
            
            if (UIEventDispatcher.Instance != null && buildingData != null)
                UIEventDispatcher.Instance.BuildingClicked(buildingData, this);
        }
        else if (uiManager != null)
        {
            uiManager.ShowInfoPanel(false);
        }
    }
    
    private void FreeAreaOnBoard()
    {
        if (board == null) return;
        board.FreeArea(occupiedGridPosition.x, occupiedGridPosition.y, size.x, size.y);
    }
    
    protected virtual void UpdateHealthBar()
    {
        if (healthBar == null) return;
        float ratio = (float)currentHP / maxHP;
        Vector3 originalScale = healthBar.localScale;
        healthBar.localScale = new Vector3(HealthBarScaleFactor * ratio, originalScale.y, originalScale.z);
    }
    
    public void DestroyBuilding()
    {
        if (spriteRenderer != null && destructedSprite != null)
        {
            spriteRenderer.sprite = destructedSprite;
        }
        
        isDestroyed = true;
        
        StartCoroutine(WaitBeforeReturnToPool());
    }
    
    private IEnumerator WaitBeforeReturnToPool()
    {
        yield return new WaitForSeconds(1f);
        if (board != null)
            board.FreeArea(occupiedGridPosition.x, occupiedGridPosition.y, size.x, size.y);
            
        BuildingPool.Instance.ReturnToPool(gameObject);
    }
    
    public void ResetBuilding()
    {
        isDestroyed = false;
        isBacksideMode = false; 
        
        if (canNotPlaceIndicator != null) 
            canNotPlaceIndicator.SetActive(false);
            
        if (selectedIndicator != null)
            selectedIndicator.SetActive(false);
        
        if (spriteRenderer != null)
        {
            if (buildingData.prefab != null && buildingData.prefab.GetComponent<SpriteRenderer>() != null)
            {
                spriteRenderer.sprite = buildingData.prefab.GetComponent<SpriteRenderer>().sprite;
            }
        }
        
        currentHP = maxHP;
        UpdateHealthBar();
        SetSelected(false);
    }
}