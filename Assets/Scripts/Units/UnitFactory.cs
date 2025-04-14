using UnityEngine;
using System.Collections.Generic;

public class UnitFactory : MonoBehaviour
{
    public static UnitFactory Instance { get; private set; }
    private Dictionary<string, int> spawnCounters = new Dictionary<string, int>();
    private float offsetRadius = 0.3f;
    private float minBuildingDistance = 0.3f;
    private int maxPositionAttempts = 8;
    
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
        
        Vector3 safePosition = FindSafeSpawnPosition(spawnPosition, spawnCount, unitData.unitName);
        
        GameObject unit;
        if (UnitPool.Instance != null)
        {
            unit = UnitPool.Instance.GetUnit(unitData.prefab, unitData.unitName);
            unit.transform.position = safePosition;
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
            unit = Instantiate(unitData.prefab, safePosition, Quaternion.identity);
            Soldier soldier = unit.GetComponent<Soldier>();
            if (soldier != null)
            {
                soldier.data = unitData;
                soldier.unitIndex = spawnCount;
            }
        }
    }
    
    private Vector3 FindSafeSpawnPosition(Vector3 basePosition, int index, string unitType)
    {
        Vector3 offsetPosition = CalculateOffset(basePosition, index, unitType);
        
        if (IsSafePosition(offsetPosition))
            return offsetPosition;
            
        float stepAngle = 360f / maxPositionAttempts;
        float currentRadius = offsetRadius;
        
        for (int attempt = 0; attempt < maxPositionAttempts; attempt++)
        {
            currentRadius += 0.1f;
            
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
        
        return basePosition + new Vector3(
            Random.Range(-0.8f, 0.8f), 
            Random.Range(-0.8f, 0.8f), 
            0
        );
    }
    
    private bool IsSafePosition(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, minBuildingDistance);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponent<BaseBuilding>() != null)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private Vector3 CalculateOffset(Vector3 basePosition, int index, string unitType)
    {
        if (index == 0) return basePosition;
        
        float angle = index * 137.5f; // Altın açı
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