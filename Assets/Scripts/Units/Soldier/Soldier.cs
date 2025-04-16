using System.Collections;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public UnitData data;
    [SerializeField] private Transform healthBar;
    
    // Components
    [HideInInspector] public SoldierHealth health;
    [HideInInspector] public SoldierMovement movement;
    [HideInInspector] public SoldierAnimator animator;
    [HideInInspector] public SoldierCombat combat;
    [HideInInspector] public SoldierSelection selection;
    
    private bool startCoroutineOnEnable = false;
    public int unitIndex = 0;
    
    private void Awake()
    {
        var soldierRenderer = GetComponentInChildren<Renderer>();
        var boardManager = FindObjectOfType<GameBoardManager>();
        
        animator = GetComponentInChildren<SoldierAnimator>();
        health = gameObject.AddComponent<SoldierHealth>();
        movement = gameObject.AddComponent<SoldierMovement>();
        combat = gameObject.AddComponent<SoldierCombat>();
        selection = GetComponent<SoldierSelection>();
        
        health.Initialize(this, data.hp, healthBar);
        movement.Initialize(this, boardManager);
        animator.Initialize(this);
        combat.Initialize(this);
        selection.Initialize(this, soldierRenderer);
    }
    
    private void Update()
    {
        if (selection.isSelected && Input.GetMouseButtonDown(1) && !health.isRespawning && health.currentHP > 0)
        {
            HandleTargetSelection();
        }
    }
    
    private void HandleTargetSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (hit.collider == null) return;

        // Building
        BaseBuilding building = hit.collider.GetComponent<BaseBuilding>();
        if (building != null)
        {
            combat.SetTargetBuilding(building);
            return;
        }
        
        // Soldier
        Soldier targetSoldier = hit.collider.GetComponent<Soldier>();
        if (targetSoldier != null && targetSoldier != this)
        {
            combat.SetTargetSoldier(targetSoldier);
            return;
        }

        // Cell
        combat.ClearTarget();
        movement.MoveTo(hit.point);
    }
    
    public void ReInitialize()
    {
        health?.StartRespawning();
        
        if (health != null && data != null)
        {
            health.currentHP = data.hp;
            health.UpdateHealthBar();
        }
        
        combat?.ClearTarget();
        movement?.StopMovement();
        selection?.Deselect();
        
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FinishRespawningAfterDelay(0.5f));
        }
        else
        {
            startCoroutineOnEnable = true;
        }
    }
    
    private void OnEnable()
    {
        if (startCoroutineOnEnable)
        {
            startCoroutineOnEnable = false;
            StartCoroutine(FinishRespawningAfterDelay(0.5f));
        }
    }
    
    private IEnumerator FinishRespawningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        health?.FinishRespawning();
    }
    
    public bool IsBeingTargeted()
    {
        foreach (Soldier s in FindObjectsOfType<Soldier>())
        {
            if (s != this && s.combat.TargetSoldier == this)
                return true;
        }
        return false;
    }
}