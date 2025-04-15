using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularScrollView : MonoBehaviour
{
    [SerializeField] private UIObjectPool objectPool;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private BuildingDatabase buildingDatabase;
    [SerializeField] private int visibleItemCount = 10;

    private int firstVisibleDataIndex = 0;
    private float itemHeight;
    private int totalDataItems;
    private List<GameObject> itemObjects = new();
    private Vector2 originalPosition;
    private bool isInitialized = false;
    private bool isShifting = false;

    void Start()
    {
        GameObject sampleItem = objectPool.GetObject();
        sampleItem.SetActive(true);
        itemHeight = sampleItem.GetComponent<RectTransform>().rect.height;
        objectPool.ReturnObject(sampleItem);

        totalDataItems = buildingDatabase.GetAllBuildings().Count;
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, itemHeight * 1.2f);
        originalPosition = Vector2.zero;

        scrollRect.onValueChanged.AddListener(OnScroll);
        InitializeItems();
        isInitialized = true;
    }

    private void InitializeItems()
    {
        foreach (var item in itemObjects)
            if (item != null) objectPool.ReturnObject(item);

        itemObjects.Clear();
        contentPanel.anchoredPosition = originalPosition;
        firstVisibleDataIndex = 0;

        for (int i = 0; i < visibleItemCount + 2; i++)
            CreateItemAtIndex(i);
    }

    private GameObject CreateItemAtIndex(int visualIndex)
    {
        int dataIndex = (firstVisibleDataIndex + visualIndex) % totalDataItems;
        GameObject itemObj = objectPool.GetObject();
        itemObj.SetActive(true);

        RectTransform rt = itemObj.GetComponent<RectTransform>();
        rt.SetParent(contentPanel, false);
        rt.anchoredPosition = new Vector2(0, -visualIndex * itemHeight);

        var building = buildingDatabase.GetAllBuildings()[dataIndex];
        var uiItem = itemObj.GetComponent<BuildingUIItem>();
        uiItem.Setup(building, null);
        uiItem.dataIndex = dataIndex;

        if (visualIndex >= itemObjects.Count)
            itemObjects.Add(itemObj);
        else
            itemObjects[visualIndex] = itemObj;

        return itemObj;
    }

    private void OnScroll(Vector2 _)
    {
        if (!isInitialized || isShifting) return;

        float scrollPos = contentPanel.anchoredPosition.y;

        if (scrollPos < 0)
        {
            isShifting = true;
            ShiftItems(true);
            contentPanel.anchoredPosition = new Vector2(0, scrollPos + itemHeight);
            isShifting = false;
        }
        else if (scrollPos > itemHeight)
        {
            isShifting = true;
            ShiftItems(false);
            contentPanel.anchoredPosition = new Vector2(0, scrollPos - itemHeight);
            isShifting = false;
        }
    }

    private void ShiftItems(bool shiftDown)
    {
        if (shiftDown)
        {
            firstVisibleDataIndex = (firstVisibleDataIndex - 1 + totalDataItems) % totalDataItems;
            var lastItem = itemObjects[^1];
            itemObjects.RemoveAt(itemObjects.Count - 1);
            itemObjects.Insert(0, lastItem);

            var building = buildingDatabase.GetAllBuildings()[firstVisibleDataIndex];
            var uiItem = lastItem.GetComponent<BuildingUIItem>();
            uiItem.Setup(building, null);
            uiItem.dataIndex = firstVisibleDataIndex;
            lastItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            int lastDataIndex = (firstVisibleDataIndex + itemObjects.Count) % totalDataItems;
            firstVisibleDataIndex = (firstVisibleDataIndex + 1) % totalDataItems;

            var firstItem = itemObjects[0];
            itemObjects.RemoveAt(0);
            itemObjects.Add(firstItem);

            var building = buildingDatabase.GetAllBuildings()[lastDataIndex];
            var uiItem = firstItem.GetComponent<BuildingUIItem>();
            uiItem.Setup(building, null);
            uiItem.dataIndex = lastDataIndex;
            firstItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(itemObjects.Count - 1) * itemHeight);
        }

        ArrangeItems();
    }

    private void ArrangeItems()
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            var rt = itemObjects[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -i * itemHeight);
        }
    }

    public void ReloadData()
    {
        totalDataItems = buildingDatabase.GetAllBuildings().Count;
        InitializeItems();
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            contentPanel.anchoredPosition = originalPosition;
            Invoke("RefreshItems", 0.05f);
        }
    }

    private void RefreshItems()
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            if (itemObjects[i] == null) continue;

            RectTransform rt = itemObjects[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -i * itemHeight);

            int dataIndex = (firstVisibleDataIndex + i) % totalDataItems;
            var building = buildingDatabase.GetAllBuildings()[dataIndex];

            var uiItem = itemObjects[i].GetComponent<BuildingUIItem>();
            uiItem.Setup(building, null);
            uiItem.dataIndex = dataIndex;
        }
    }
}
