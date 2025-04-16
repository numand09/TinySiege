using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUIItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public int dataIndex;

    private BuildingData data;
    private BaseBuilding instance;
    private Button button;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
    }

    public void Setup(BuildingData buildingData, BaseBuilding buildingInstance)
    {
        data = buildingData;
        instance = buildingInstance;

        if (data == null) return;

        iconImage.sprite = data.icon;
        nameText.text = data.buildingName;
        if (hpText != null) hpText.text = "HP: " + data.maxHP;

        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            UIEventDispatcher.Instance?.BuildingClicked(data, instance);
        });
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveAllListeners();
    }
}
