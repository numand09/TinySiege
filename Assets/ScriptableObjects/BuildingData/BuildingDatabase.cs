using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDatabase", menuName = "RTS/Building Database")]
public class BuildingDatabase : ScriptableObject
{
    public List<BuildingData> buildingList;

    public BuildingData GetBuildingByName(string name)
    {
        return buildingList.Find(b => b.buildingName == name);
    }

    public List<BuildingData> GetAllBuildings()
    {
        return buildingList;
    }
}
