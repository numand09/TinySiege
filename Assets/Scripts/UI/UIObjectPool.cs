using System.Collections.Generic;
using UnityEngine;

public class UIObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 20;
    
    private List<GameObject> pooledObjects = new List<GameObject>();
    
    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }
    
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }
    
    public GameObject GetObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        
        return CreateNewObject();
    }
    
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
    
    public void ReturnAllObjects()
    {
        foreach (GameObject obj in pooledObjects)
        {
            obj.SetActive(false);
        }
    }
}