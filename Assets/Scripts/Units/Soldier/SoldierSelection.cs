using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class SoldierSelection : MonoBehaviour
{
    private Soldier soldier;
    private Renderer soldierRenderer;
    public GameObject rangeIndicator;
    public bool isSelected = false;
    public LayerMask groundLayerMask;
    [SerializeField] private static RectTransform selectionBoxUI;
    private Vector2 startMousePos;
    
    private static List<SoldierSelection> allSelectedSoldiers = new List<SoldierSelection>();
    
    private static List<SoldierSelection> allSoldiers = new List<SoldierSelection>();
    
    private static Vector2 dragStartPosition;
    private static bool isDragging = false;
    private static GameObject selectionBox;
    
private void Awake()
{
    if (selectionBoxUI == null)
    {
        GameObject found = GameObject.Find("SelectionBox");
        if (found != null)
        {
            selectionBoxUI = found.GetComponent<RectTransform>();
            selectionBoxUI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("SelectionBox UI nesnesi sahnede bulunamadı.");
        }
    }
}
    private void OnEnable()
    {
        if (!allSoldiers.Contains(this))
            allSoldiers.Add(this);
    }
    
    private void OnDisable()
    {
        allSoldiers.Remove(this);
        if (isSelected)
            allSelectedSoldiers.Remove(this);
    }
    
    public void Initialize(Soldier soldierRef, Renderer rendererRef)
    {
        soldier = soldierRef;
        soldierRenderer = rendererRef;
        rangeIndicator = soldier.transform.Find("Range")?.gameObject;
        
        if (rangeIndicator != null)
            rangeIndicator.SetActive(false);
        
        if (selectionBox == null)
            selectionBox = CreateSelectionBox();
    }
    
   void Update()
{
    if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
    {
        startMousePos = Input.mousePosition;
        isDragging = true;
        selectionBoxUI.gameObject.SetActive(true);
    }

    if (isDragging)
    {
        Vector2 currentMousePos = Input.mousePosition;
        UpdateSelectionBoxUI(startMousePos, currentMousePos);
    }

    if (Input.GetMouseButtonUp(0))
    {
        if (isDragging)
        {
            Vector2 endMousePos = Input.mousePosition;

            if (Vector2.Distance(startMousePos, endMousePos) < 5f) // Küçük tıklama
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

                if (hit.collider != null)
                {
                    SoldierSelection clickedSoldier = hit.collider.GetComponent<SoldierSelection>();
                    if (clickedSoldier == null && (groundLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                        DeselectAllSoldiers();
                }
                else
                {
                    DeselectAllSoldiers();
                }
            }
            else
            {
                Vector2 worldStart = Camera.main.ScreenToWorldPoint(startMousePos);
                Vector2 worldEnd = Camera.main.ScreenToWorldPoint(endMousePos);
                SelectSoldiersInRectangle(worldStart, worldEnd);
            }

            isDragging = false;
            selectionBoxUI.gameObject.SetActive(false);
        }
    }
}

    private void UpdateSelectionBoxUI(Vector2 start, Vector2 end)
{
    Vector2 size = new Vector2(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
    Vector2 pos = start + (end - start) / 2f;

    selectionBoxUI.position = pos;
    selectionBoxUI.sizeDelta = size;
}

    private static GameObject CreateSelectionBox()
    {
        GameObject selectionBoxObj = new GameObject("SelectionBox");
        selectionBoxObj.transform.SetParent(null);
        
        SpriteRenderer renderer = selectionBoxObj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        renderer.sprite = sprite;
        
        selectionBoxObj.SetActive(false);
        return selectionBoxObj;
    }
    
    private static void UpdateSelectionBox(Vector2 startPos, Vector2 currentPos)
    {
        Vector2 center = (startPos + currentPos) / 2f;
        Vector2 size = new Vector2(
            Mathf.Abs(currentPos.x - startPos.x),
            Mathf.Abs(currentPos.y - startPos.y)
        );
        
        selectionBox.transform.position = center;
        selectionBox.transform.localScale = size;
    }
    
    private static void SelectSoldiersInRectangle(Vector2 startPos, Vector2 endPos)
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            DeselectAllSoldiers();
        
        Rect selectionRect = new Rect(
            Mathf.Min(startPos.x, endPos.x),
            Mathf.Min(startPos.y, endPos.y),
            Mathf.Abs(endPos.x - startPos.x),
            Mathf.Abs(endPos.y - startPos.y)
        );
        
        foreach (SoldierSelection soldier in allSoldiers)
        {
            Vector2 soldierPosition = soldier.transform.position;
            
            if (selectionRect.Contains(soldierPosition) && !soldier.isSelected)
                soldier.ToggleSelection();
        }
    }
    
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        
        // Sadece Range göstergesini kullan
        if (rangeIndicator != null)
            rangeIndicator.SetActive(isSelected);
        
        if (isSelected)
        {
            if (!allSelectedSoldiers.Contains(this))
                allSelectedSoldiers.Add(this);
        }
        else
        {
            allSelectedSoldiers.Remove(this);
        }
    }
    
    public void Deselect()
    {
        if (isSelected)
        {
            isSelected = false;
            
            if (rangeIndicator != null)
                rangeIndicator.SetActive(false);
            
            allSelectedSoldiers.Remove(this);
        }
    }
    
    private static void DeselectAllSoldiers()
    {
        List<SoldierSelection> selectedSoldiersCopy = new List<SoldierSelection>(allSelectedSoldiers);
        foreach (var soldier in selectedSoldiersCopy)
            soldier.Deselect();
        
        allSelectedSoldiers.Clear();
    }
    
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}