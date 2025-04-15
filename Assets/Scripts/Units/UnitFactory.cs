using UnityEngine;
using System.Collections.Generic;
using System;

public class UnitFactory : MonoBehaviour
{
    public static UnitFactory Instance { get; private set; }
    private Dictionary<string, int> spawnCounters = new Dictionary<string, int>();
    private float offsetRadius = 0.3f;
    private float minBuildingDistance = 0.3f;
    private int maxPositionAttempts = 8;
    
    [SerializeField] private LayerMask buildingsLayerMask;
    
    // Event to notify UI when spawn area is blocked
    public event Action OnSpawnAreaBlocked;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    
    public void SpawnUnit(UnitData unitData, Vector3 spawnPosition)
    {
        if (!spawnCounters.ContainsKey(unitData.unitName))
            spawnCounters[unitData.unitName] = 0;
            
        int spawnCount = spawnCounters[unitData.unitName]++;
        
        Vector3? safePosition = FindSafeSpawnPosition(spawnPosition, spawnCount, unitData.unitName);
        
        // If no safe position found, trigger the event and return
        if (!safePosition.HasValue)
        {
            OnSpawnAreaBlocked?.Invoke();
            return;
        }
        
        GameObject unit;
        if (UnitPool.Instance != null)
        {
            unit = UnitPool.Instance.GetUnit(unitData.prefab, unitData.unitName);
            unit.transform.position = safePosition.Value;
            Soldier soldier = unit.GetComponent<Soldier>();
            if (soldier != null)
            {
                soldier.data = unitData;
                if (soldier.health != null)
                {
                    soldier.health.currentHP = unitData.hp;
                    soldier.health.UpdateHealthBar();
                }
                soldier.unitIndex = spawnCount;
                Renderer soldierRenderer = unit.GetComponentInChildren<Renderer>();
                if (soldierRenderer != null)
                {
                    Color currentColor = soldierRenderer.material.color;
                    soldierRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
                }
            }
        }
        else
        {
            unit = Instantiate(unitData.prefab, safePosition.Value, Quaternion.identity);
            Soldier soldier = unit.GetComponent<Soldier>();
            if (soldier != null)
            {
                soldier.data = unitData;
                soldier.unitIndex = spawnCount;
            }
        }
    }
    
    private Vector3? FindSafeSpawnPosition(Vector3 basePosition, int index, string unitType)
    {
        Vector3 offsetPosition = CalculateOffset(basePosition, index, unitType);
        
        if (IsSafePosition(offsetPosition))
            return offsetPosition;
            
        float stepAngle = 360f / maxPositionAttempts;
        float currentRadius = offsetRadius;
        
        // Increase the maximum attempts to find a safe position
        int increasedMaxAttempts = maxPositionAttempts * 3;
        
        for (int attempt = 0; attempt < increasedMaxAttempts; attempt++)
        {
            // Increase radius more gradually to try more positions
            currentRadius += 0.2f;
            
            float angle = (index * 137.5f) + (attempt * stepAngle);
            
            int typeOffset = 0;
            for (int i = 0; i < unitType.Length; i++)
                typeOffset += unitType[i];
            angle += typeOffset % 60;
            
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * currentRadius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * currentRadius;
            Vector3 testPosition = basePosition + new Vector3(x, y, 0);
            
            if (IsSafePosition(testPosition))
                return testPosition;
        }
        
        // Try a few random positions as a last resort
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPosition = basePosition + new Vector3(
                UnityEngine.Random.Range(-2f, 2f), 
                UnityEngine.Random.Range(-2f, 2f), 
                0
            );
            
            if (IsSafePosition(randomPosition))
                return randomPosition;
        }
        
        // If no safe position found at all, return null
        return null;
    }
    
    private bool IsSafePosition(Vector3 position)
    {
        // Check for objects in the Buildings layer within a 2 unit radius
        float checkRadius = 2f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius, buildingsLayerMask);
        
        foreach (Collider2D collider in colliders)
        {
            // Calculate the actual distance between the position and the collider's position
            float distance = Vector2.Distance(position, collider.transform.position);
            
            // Get the bounds of the collider to determine a safe distance
            float minSafeDistance = minBuildingDistance;
            
            // If collider has a size, use that to determine a more accurate safe distance
            BoxCollider2D boxCollider = collider.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                // Use the max dimension of the collider plus our minimum distance
                float maxDimension = Mathf.Max(boxCollider.size.x, boxCollider.size.y) / 2f;
                minSafeDistance = maxDimension + minBuildingDistance;
            }
            
            // If any object in the Buildings layer is too close, this is not a safe position
            if (distance < minSafeDistance)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private Vector3 CalculateOffset(Vector3 basePosition, int index, string unitType)
    {
        if (index == 0) return basePosition;
        
        float angle = index * 137.5f;
        float distance = Mathf.Sqrt(index) * (offsetRadius / 2f);
        
        int typeOffset = 0;
        for (int i = 0; i < unitType.Length; i++)
            typeOffset += unitType[i];
        angle += typeOffset % 60;
        
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;
        
        return basePosition + new Vector3(x, y, 0);
    }
}