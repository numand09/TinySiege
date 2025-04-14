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

    private void Awake()
    {
        // Get references
        soldierRenderer = GetComponentInChildren<Renderer>();
        boardManager = FindObjectOfType<GameBoardManager>();
        animator = gameObject.GetComponentInChildren<SoldierAnimator>();
        
        // Initialize components
        health = gameObject.AddComponent<SoldierHealth>();
        movement = gameObject.AddComponent<SoldierMovement>();
        combat = gameObject.AddComponent<SoldierCombat>();
        selection = gameObject.GetComponent<SoldierSelection>();
        
        // Setup dependencies
        health.Initialize(this, data.hp, healthBar);
        movement.Initialize(this, boardManager);
        animator.Initialize(this);
        combat.Initialize(this);
        selection.Initialize(this, soldierRenderer);
    }

    private void Update()
    {
        // Handle selection input
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
                // We clicked on a building - attack it
                combat.SetTargetBuilding(building);
            }
            else
            {
                // We clicked on something else - move there
                combat.ClearTarget();
                movement.MoveTo(hit.point);
            }
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
    // Mark as respawning to prevent being targeted
    if (health != null)
    {
        health.StartRespawning();
    }
    
    // Reset health
    if (health != null && data != null)
    {
        health.currentHP = data.hp;
        health.UpdateHealthBar();
    }
    
    // Reset combat state
    if (combat != null)
    {
        combat.ClearTarget();
    }
    
    // Reset movement
    if (movement != null)
    {
        movement.StopMovement();
    }
    
    // Reset selection
    if (selection != null)
    {
        selection.Deselect();
    }
    
    // Reset renderer material opacity
    Renderer[] renderers = GetComponentsInChildren<Renderer>();
    foreach (Renderer renderer in renderers)
    {
        if (renderer != null && renderer.material != null)
        {
            Color c = renderer.material.color;
            renderer.material.color = new Color(c.r, c.g, c.b, 1f); // Full opacity
        }
    }
    
    // Don't start the coroutine if the GameObject is inactive
    // Instead, we'll finish respawning right away or set up a delayed method call after activation
    if (gameObject.activeInHierarchy)
    {
        StartCoroutine(FinishRespawningAfterDelay(0.5f));
    }
    else
    {
        // Store that we need to finish respawning when activated
        StartCoroutineOnEnable = true;
    }
}

// Add this field to the Soldier class
private bool StartCoroutineOnEnable = false;

// Add this method to the Soldier class
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
}