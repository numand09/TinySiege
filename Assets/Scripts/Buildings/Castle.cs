using UnityEngine;

public class Castle : BaseBuilding
{
    protected override float HealthBarScaleFactor => 0.5f;

    void Start()
    {
        buildingName = "Castle";
        size = new Vector2Int(6, 6);
        maxHP = 300;
        Initialize();
    }
}
