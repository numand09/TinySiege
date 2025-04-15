using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public UnitData data;
    [SerializeField] private Transform healthBar;
    
    // References to components
    private Renderer soldierRenderer;
    private GameBoardManager boardManager;
    
    // Component references
    [HideInInspector] public SoldierHealth health;
    [HideInInspector] public SoldierMovement movement;
    [HideInInspector] public SoldierAnimator animator;
    [HideInInspector] public SoldierCombat combat;
    [HideInInspector] public SoldierSelection selection;
    
    private bool StartCoroutineOnEnable = false;
    public int unitIndex = 0;
    [SerializeField] private LayerMask soldierLayerMask;
    
    private void Awake()
    {
        soldierRenderer = GetComponentInChildren<Renderer>();
        boardManager = FindObjectOfType<GameBoardManager>();
        
        animator = gameObject.GetComponentInChildren<SoldierAnimator>();
        health = gameObject.AddComponent<SoldierHealth>();
        movement = gameObject.AddComponent<SoldierMovement>();
        combat = gameObject.AddComponent<SoldierCombat>();
        selection = gameObject.GetComponent<SoldierSelection>();
        
        health.Initialize(this, data.hp, healthBar);
        movement.Initialize(this, boardManager);
        animator.Initialize(this);
        combat.Initialize(this);
        selection.Initialize(this, soldierRenderer);
    }
    
    private void Update()
    {
        if (selection.isSelected && Input.GetMouseButtonDown(1))
        {
            HandleTargetSelection();
        }
    }
    
    private void HandleTargetSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (hit.collider != null)
        {
            BaseBuilding building = hit.collider.GetComponent<BaseBuilding>();
            if (building != null)
            {
                combat.SetTargetBuilding(building);
                return;
            }
            
            Soldier targetSoldier = hit.collider.GetComponent<Soldier>();
            if (targetSoldier != null && targetSoldier != this)
            {
                combat.SetTargetSoldier(targetSoldier);
                return;
            }
            combat.ClearTarget();
            movement.MoveTo(hit.point);
        }
    }
    
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selection.ToggleSelection();
        }
    }
    
    public void ReInitialize()
    {
        if (health != null)
        {
            health.StartRespawning();
        }
        
        if (health != null && data != null)
        {
            health.currentHP = data.hp;
            health.UpdateHealthBar();
        }
        
        if (combat != null)
        {
            combat.ClearTarget();
        }
        
        if (movement != null)
        {
            movement.StopMovement();
        }
        
        if (selection != null)
        {
            selection.Deselect();
        }
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Color c = renderer.material.color;
                renderer.material.color = new Color(c.r, c.g, c.b, 1f); // Full opacity
            }
        }
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FinishRespawningAfterDelay(0.5f));
        }
        else
        {
            StartCoroutineOnEnable = true;
        }
    }
    
    private void OnEnable()
    {
        if (StartCoroutineOnEnable)
        {
            StartCoroutineOnEnable = false;
            StartCoroutine(FinishRespawningAfterDelay(0.5f));
        }
    }
    
    private IEnumerator FinishRespawningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (health != null)
        {
            health.FinishRespawning();
        }
    }
    
    public bool IsBeingTargeted()
    {
        Soldier[] allSoldiers = FindObjectsOfType<Soldier>();
        foreach (Soldier s in allSoldiers)
        {
            if (s != this && s.combat.TargetSoldier == this)
            {
                return true;
            }
        }
        return false;
    }
}