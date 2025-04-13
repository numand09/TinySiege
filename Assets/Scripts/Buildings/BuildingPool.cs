using System.Collections.Generic;
using UnityEngine;
public class BuildingPool : MonoBehaviour
{
    public static BuildingPool Instance;
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public GameObject GetFromPool(BuildingData data, Vector3 position, Quaternion rotation)
    {
        string key = data.buildingName;
        if (!poolDictionary.ContainsKey(key) || poolDictionary[key].Count == 0)
        {
            // Havuzda bina yok, yenisini oluştur
            GameObject newBuilding = Instantiate(data.prefab, position, rotation);
            newBuilding.SetActive(true);
            return newBuilding;
        }
        else
        {
            // Havuzdan al
            GameObject pooled = poolDictionary[key].Dequeue();
            pooled.transform.position = position;
            pooled.transform.rotation = rotation;
            
            // Enable all colliders when object is taken from pool
            EnableColliders(pooled, true);
            
            pooled.SetActive(true);
            return pooled;
        }
    }
    
    public void ReturnToPool(GameObject building)
    {
        if (building == null) return;
        
        string key = building.GetComponent<BaseBuilding>().buildingData.buildingName;
        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();
        
        // Disable all colliders before returning to pool
        EnableColliders(building, false);
        
        building.SetActive(false);
        poolDictionary[key].Enqueue(building);
    }
    
    // Helper method to enable or disable all colliders on an object
    private void EnableColliders(GameObject obj, bool enable)
    {
        // Get all collider components (BoxCollider2D, CircleCollider2D, etc.)
        Collider2D[] colliders = obj.GetComponents<Collider2D>();
        
        // Also check child objects for colliders
        Collider2D[] childColliders = obj.GetComponentsInChildren<Collider2D>();
        
        // Enable/disable all found colliders
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = enable;
        }
        
        foreach (Collider2D collider in childColliders)
        {
            collider.enabled = enable;
        }
    }
}