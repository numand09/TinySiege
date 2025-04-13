using System.Collections.Generic;
using UnityEngine;

public class UnitPool : MonoBehaviour
{
    public static UnitPool Instance { get; private set; }
    
    // Dictionary to store pools for different unit types
    private Dictionary<string, Queue<GameObject>> unitPools = new Dictionary<string, Queue<GameObject>>();
    
    // Parent transform to organize pooled objects
    [SerializeField] private Transform poolParent;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        // Create the parent if it doesn't exist
        if (poolParent == null)
        {
            poolParent = new GameObject("PooledUnits").transform;
            poolParent.SetParent(transform);
        }
    }
    
    public GameObject GetUnit(GameObject unitPrefab, string unitId)
    {
        // Try to get from pool
        if (unitPools.ContainsKey(unitId) && unitPools[unitId].Count > 0)
        {
            GameObject pooledUnit = unitPools[unitId].Dequeue();
            
            // Activate the unit BEFORE reinitializing
            pooledUnit.SetActive(true);
            
            // Reset state
            ResetUnitState(pooledUnit);
            
            return pooledUnit;
        }
        
        // No units in pool, create new one
        GameObject newUnit = Instantiate(unitPrefab);
        return newUnit;
    }
    
    private void ResetUnitState(GameObject unit)
    {
        // The unit should already be active at this point
        
        // Get the Soldier component and reset its state
        Soldier soldier = unit.GetComponent<Soldier>();
        if (soldier != null)
        {
            soldier.ReInitialize();
        }
        
        // Any other components that need resetting
    }
    
    public void ReturnToPool(GameObject unit, string unitId)
    {
        // Make sure we have a pool for this unit type
        if (!unitPools.ContainsKey(unitId))
        {
            unitPools[unitId] = new Queue<GameObject>();
        }
        
        // Reset the unit state
        unit.SetActive(false);
        unit.transform.SetParent(poolParent);
        
        // Add to the appropriate pool
        unitPools[unitId].Enqueue(unit);
    }
}