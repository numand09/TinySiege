using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSelection : MonoBehaviour
{
    private Soldier soldier;
    private Renderer soldierRenderer;
    private GameObject rangeIndicator;
    public bool isSelected = false;

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

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        soldierRenderer.material.color = isSelected ? Color.yellow : Color.white;
        
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(isSelected);
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
        }
    }
}