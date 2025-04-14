using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoldierSelection : MonoBehaviour
{
    private Soldier soldier;
    private Renderer soldierRenderer;
    private GameObject rangeIndicator;
    public bool isSelected = false;    
    public LayerMask groundLayerMask;
    private static List<SoldierSelection> allSelectedSoldiers = new List<SoldierSelection>();
    public void Initialize(Soldier soldierRef, Renderer rendererRef)
    {
        soldier = soldierRef;
        soldierRenderer = rendererRef;
        rangeIndicator = soldier.transform.Find("Range")?.gameObject;
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            
            if (hit.collider != null)
            {
                SoldierSelection clickedSoldier = hit.collider.GetComponent<SoldierSelection>();
                if (clickedSoldier == null)
                {
                    if ((groundLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                    {
                        DeselectAllSoldiers();
                    }
                }
            }
            else
            {
                DeselectAllSoldiers();
            }
        }
    }
    
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        soldierRenderer.material.color = isSelected ? Color.yellow : Color.white;
        
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(isSelected);
        }
                if (isSelected)
        {
            if (!allSelectedSoldiers.Contains(this))
            {
                allSelectedSoldiers.Add(this);
            }
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
            soldierRenderer.material.color = Color.white;
            isSelected = false;
            
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(false);
            }
            
            allSelectedSoldiers.Remove(this);
        }
    }
    
    private static void DeselectAllSoldiers()
    {
        // For avoid modification during iteration
        List<SoldierSelection> selectedSoldiersCopy = new List<SoldierSelection>(allSelectedSoldiers);
        
        foreach (var soldier in selectedSoldiersCopy)
        {
            soldier.Deselect();
        }
        
        allSelectedSoldiers.Clear();
    }
    
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}