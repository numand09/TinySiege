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
    private List<GameObject> itemObjects = new List<GameObject>();
    private float contentHeight;
    private Vector2 originalPosition;
    private bool isInitialized = false;
    
    void Start()
    {
        GameObject sampleItem = objectPool.GetObject();
        sampleItem.SetActive(true);
        itemHeight = sampleItem.GetComponent<RectTransform>().rect.height;
        objectPool.ReturnObject(sampleItem);
        
        totalDataItems = buildingDatabase.GetAllBuildings().Count;
        
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, itemHeight * (visibleItemCount + 2));
        contentHeight = contentPanel.sizeDelta.y;
        originalPosition = contentPanel.anchoredPosition;
        
        scrollRect.onValueChanged.AddListener(OnScroll);
        
        InitializeItems();
        
        isInitialized = true;
    }
    
    private void InitializeItems()
{
    // Clear previous items
    foreach (var item in itemObjects)
    {
        if (item != null)
            objectPool.ReturnObject(item);
    }
    
    itemObjects.Clear();
    
    firstVisibleDataIndex = 0;
    
    for (int i = 0; i < visibleItemCount + 2; i++)
    {
        GameObject itemObj = objectPool.GetObject();
        itemObj.SetActive(true);
        
        RectTransform rt = itemObj.GetComponent<RectTransform>();
        rt.SetParent(contentPanel, false);
        rt.anchoredPosition = new Vector2(0, -i * itemHeight);
        
        int dataIndex = (firstVisibleDataIndex + i) % totalDataItems;
        BuildingData building = buildingDatabase.GetAllBuildings()[dataIndex];
        itemObj.GetComponent<BuildingUIItem>().Setup(building, null);
        
        itemObjects.Add(itemObj);
    }
}
    
    private void OnScroll(Vector2 normalizedPos)
    {
        if (!isInitialized) return;
        
        float scrollPos = contentPanel.anchoredPosition.y;
        
        if (scrollPos < 0)
        {
            ShiftItems(true);
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, scrollPos + itemHeight);
        }
        else if (scrollPos > itemHeight * (visibleItemCount - 2))
        {
            ShiftItems(false);
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, scrollPos - itemHeight);
        }
    }
    

private void ShiftItems(bool shiftDown)
{
    if (shiftDown)
    {
        firstVisibleDataIndex = (firstVisibleDataIndex - 1 + totalDataItems) % totalDataItems;
        
        GameObject lastItem = itemObjects[itemObjects.Count - 1];
        itemObjects.RemoveAt(itemObjects.Count - 1);
        itemObjects.Insert(0, lastItem);
        
        BuildingData building = buildingDatabase.GetAllBuildings()[firstVisibleDataIndex];
        lastItem.GetComponent<BuildingUIItem>().Setup(building, null);
        
        RectTransform rt = lastItem.GetComponent<RectTransform>();
        RectTransform firstRT = itemObjects[1].GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, firstRT.anchoredPosition.y + itemHeight);
    }
    else
    {
        firstVisibleDataIndex = (firstVisibleDataIndex + 1) % totalDataItems;
        
        GameObject firstItem = itemObjects[0];
        itemObjects.RemoveAt(0);
        itemObjects.Add(firstItem);
        
        int lastIndex = (firstVisibleDataIndex + visibleItemCount) % totalDataItems;
        BuildingData building = buildingDatabase.GetAllBuildings()[lastIndex];
        firstItem.GetComponent<BuildingUIItem>().Setup(building, null);
        
        RectTransform rt = firstItem.GetComponent<RectTransform>();
        RectTransform lastRT = itemObjects[itemObjects.Count - 2].GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, lastRT.anchoredPosition.y - itemHeight);
    }
    
    ArrangeItems();
}
    
    private void ArrangeItems()
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            RectTransform rt = itemObjects[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -i * itemHeight);
        }
    }
    
    private int GetDataIndex(int itemIndex)
    {
        int firstVisibleDataIndex = 0;
        
        for (int i = 0; i < itemObjects.Count; i++)
        {
            BuildingUIItem item = itemObjects[i].GetComponent<BuildingUIItem>();
            if (item.nameText.text == buildingDatabase.GetAllBuildings()[firstVisibleDataIndex].buildingName)
            {
                int offset = itemIndex - i;
                int dataIndex = (firstVisibleDataIndex + offset) % totalDataItems;
                if (dataIndex < 0) dataIndex += totalDataItems;
                return dataIndex;
            }
        }
        
        return itemIndex % totalDataItems;
    }
    
    public void ReloadData()
    {
        totalDataItems = buildingDatabase.GetAllBuildings().Count;
        contentPanel.anchoredPosition = originalPosition;
        InitializeItems();
    }
}