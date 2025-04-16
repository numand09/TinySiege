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
    
    private static RectTransform selectionBoxUI;
    private static bool isDragging = false;
    private static Vector2 startMousePos;
    
    private static List<SoldierSelection> allSelectedSoldiers = new List<SoldierSelection>();
    private static List<SoldierSelection> allSoldiers = new List<SoldierSelection>();
    
    public void Initialize(Soldier soldierRef, Renderer rendererRef)
    {
        soldier = soldierRef;
        soldierRenderer = rendererRef;
        rangeIndicator = soldier.transform.Find("Range")?.gameObject;
        
        if (rangeIndicator != null)
            rangeIndicator.SetActive(false);
            
        if (!allSoldiers.Contains(this))
            allSoldiers.Add(this);
            
        InitializeSelectionBox();
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
    
    private void InitializeSelectionBox()
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
                Debug.LogWarning("SelectionBox UI object not found in the scene.");
            }
        }
    }
    
    void Update()
    {
        HandleSelectionInput();
    }
    
    private void HandleSelectionInput()
    {
        // Start selection box when mouse button is pressed
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            startMousePos = Input.mousePosition;
            isDragging = true;
            selectionBoxUI.gameObject.SetActive(true);
        }

        // Update selection box while dragging
        if (isDragging)
        {
            Vector2 currentMousePos = Input.mousePosition;
            UpdateSelectionBoxUI(startMousePos, currentMousePos);
        }

        // Finalize selection when mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                Vector2 endMousePos = Input.mousePosition;
                FinalizeSelection(endMousePos);
                
                isDragging = false;
                selectionBoxUI.gameObject.SetActive(false);
            }
        }
    }
    
    private void FinalizeSelection(Vector2 endMousePos)
    {
        // Is it a simple click? (Point selection)
        if (Vector2.Distance(startMousePos, endMousePos) < 5f)
        {
            HandlePointSelection();
        }
        else // Rectangle selection
        {
            Vector2 worldStart = Camera.main.ScreenToWorldPoint(startMousePos);
            Vector2 worldEnd = Camera.main.ScreenToWorldPoint(endMousePos);
            SelectSoldiersInRectangle(worldStart, worldEnd);
        }
    }
    
    private void HandlePointSelection()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            SoldierSelection clickedSoldier = hit.collider.GetComponent<SoldierSelection>();
            if (clickedSoldier != null)
            {
                // If Shift is not held, clear previous selections
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    DeselectAllSoldiers();
                    
                // Select the soldier
                clickedSoldier.ToggleSelection();
                return;
            }
            else if ((groundLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                // If ground clicked, clear selection
                DeselectAllSoldiers();
            }
        }
        else
        {
            // If empty space clicked, clear selection
            DeselectAllSoldiers();
        }
    }

    private void UpdateSelectionBoxUI(Vector2 start, Vector2 end)
    {
        Vector2 size = new Vector2(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
        Vector2 pos = start + (end - start) / 2f;
        
        selectionBoxUI.position = pos;
        selectionBoxUI.sizeDelta = size;
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
