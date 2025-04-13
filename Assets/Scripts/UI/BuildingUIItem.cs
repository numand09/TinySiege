using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUIItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText; 
    private BuildingData data;
    private BaseBuilding instance;

    public void Setup(BuildingData buildingData, BaseBuilding buildingInstance)
    {
        data = buildingData;
        instance = buildingInstance;

        iconImage.sprite = data.icon;
        nameText.text = data.buildingName;

        if (hpText != null)
        {
            hpText.text = "HP: " + data.maxHP.ToString();
        }

        GetComponentInChildren<Button>().onClick.AddListener(() => {
            UIEventDispatcher.Instance.BuildingItemClicked(data, buildingInstance);
        });
    }

    private void OnDestroy()
    {
        // For memory leak
        GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }
}