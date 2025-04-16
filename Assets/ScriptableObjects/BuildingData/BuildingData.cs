using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "RTS/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public GameObject prefab;
    public int maxHP;
    public List<UnitData> trainableUnits;
    public Sprite backsideSprite;
}