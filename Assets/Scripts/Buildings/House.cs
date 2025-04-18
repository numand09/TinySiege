using UnityEngine;

public class House : BaseBuilding
{
    protected override float HealthBarScaleFactor => 0.5f;

    void Start()
    {
        buildingName = "House";
        size = new Vector2Int(4, 4);
        maxHP = 300;
        Initialize();
    }
}
