using UnityEngine;

public class UnitFactory : MonoBehaviour
{
    public static UnitFactory Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    
    public void SpawnUnit(UnitData unitData, Vector3 spawnPosition)
{
    GameObject unit;
    
    if (UnitPool.Instance != null)
    {
        unit = UnitPool.Instance.GetUnit(unitData.prefab, unitData.unitName);
        unit.transform.position = spawnPosition;
        
        Soldier soldier = unit.GetComponent<Soldier>();
        if (soldier != null)
        {
            soldier.data = unitData;
            
            if (soldier.health != null)
            {
                soldier.health.currentHP = unitData.hp;
                soldier.health.UpdateHealthBar();
            }
            
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
        unit = Instantiate(unitData.prefab, spawnPosition, Quaternion.identity);
        Soldier soldier = unit.GetComponent<Soldier>();
        if (soldier != null)
        {
            soldier.data = unitData;
        }
    }
}
}