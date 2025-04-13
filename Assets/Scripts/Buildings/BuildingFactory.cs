using UnityEngine;

public class BuildingFactory : MonoBehaviour
{
    public static BuildingFactory Instance;

    public GameObject barracksPrefab;
    public GameObject powerPlantPrefab;

    void Awake()
    {
        Instance = this;
    }

    public GameObject CreateBuilding(string type)
    {
        switch (type)
        {
            case "Barrack":
                return Instantiate(barracksPrefab);
            case "PowerPlant":
                return Instantiate(powerPlantPrefab);
            default:
                return null;
        }
    }
}