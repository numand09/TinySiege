using UnityEngine;

public class PowerPlant : BaseBuilding
{
    void Start()
    {
        buildingName = "Power Plant";
        size = new Vector2Int(2, 3);
        maxHP = 50;
        Initialize();
    }
    protected override float HealthBarScaleFactor => 1f;

}
