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
            spriteRenderer.sprite = destructedSprite;
            StartCoroutine(FadeAndDestroy());
        }
    }
    
    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(2f);
        Renderer buildingRenderer = GetComponent<Renderer>();
        Color startColor = buildingRenderer.material.color;
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
            buildingRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
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
        StartCoroutine(FadeOutAndReturnToPool());
    }
    
    private IEnumerator FadeOutAndReturnToPool()
    {
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.material.color;
            float fadeDuration = 1f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
                spriteRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        if (board != null)
            board.FreeArea(occupiedGridPosition.x, occupiedGridPosition.y, size.x, size.y);        
        BuildingPool.Instance.ReturnToPool(gameObject);
    }
    
    public void ResetBuilding()
    {
        isDestroyed = false;
        isBacksideMode = false; 
        
        // Hide indicators when resetting
        if (canNotPlaceIndicator != null) 
            canNotPlaceIndicator.SetActive(false);
            
        if (selectedIndicator != null)
            selectedIndicator.SetActive(false);
        
        if (spriteRenderer != null)
        {            
            Color color = spriteRenderer.material.color;
            color.a = 0;
            spriteRenderer.material.color = color;
            
            if (buildingData.prefab != null && buildingData.prefab.GetComponent<SpriteRenderer>() != null)
            {
                spriteRenderer.sprite = buildingData.prefab.GetComponent<SpriteRenderer>().sprite;
            }
            
            StartCoroutine(FadeIn());
        }
        
        currentHP = maxHP;
        UpdateHealthBar();
        SetSelected(false);
    }
    
    private IEnumerator FadeIn()
    {
        if (spriteRenderer != null)
        {
            Color targetColor = spriteRenderer.material.color;
            targetColor.a = 1f;
            Color currentColor = spriteRenderer.material.color;
            float fadeDuration = 1f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(0f, targetColor.a, elapsedTime / fadeDuration);
                spriteRenderer.material.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}