using UnityEngine;

public class Barrack : BaseBuilding
{

    protected override float HealthBarScaleFactor => 0.5f;

    void Start()
    {
        buildingName = "Barrack";
        size = new Vector2Int(4, 4);
        maxHP = 100;
        Initialize();
    }

}
