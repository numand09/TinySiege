using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierHealth : MonoBehaviour
{
    private Soldier soldier;
    private int maxHP;
    public int currentHP;
    private Transform healthBar;
    [HideInInspector]
    public bool isRespawning = false;

    public void Initialize(Soldier soldierRef, int startingHP, Transform healthBarRef)
    {
        soldier = soldierRef;
        maxHP = startingHP;
        currentHP = startingHP;
        healthBar = healthBarRef;
    }

    public void TakeDamage(int dmg)
    {
        if (isRespawning)
            return;

        currentHP -= dmg;
        UpdateHealthBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void UpdateHealthBar()
    {
        if (healthBar == null) return;
        float ratio = (float)currentHP / maxHP;
        Vector3 originalScale = healthBar.localScale;
        healthBar.localScale = new Vector3(0.3f * ratio, originalScale.y, originalScale.z);
    }

    public void Die()
    { 
        soldier.selection.rangeIndicator.SetActive(false);
        
        SoldierCombat combat = soldier.GetComponent<SoldierCombat>();
        if (combat != null)
        {
            combat.ClearTarget();
        }
        
        SoldierMovement movement = soldier.GetComponent<SoldierMovement>();
        if (movement != null)
        {
            movement.StopMovement();
        }
        
        soldier.animator.PlayDeathAnimation();
        StartCoroutine(DeathAndDestroy());
    }

    private IEnumerator DeathAndDestroy()
    {
        isRespawning = true;
        float deathWaitTime = 2.0f;
        if (soldier.animator.deathClip != null)
        {
            deathWaitTime = soldier.animator.deathClip.length + 0.5f; 
        }
        
        yield return new WaitForSeconds(deathWaitTime);
        
        if (UnitPool.Instance != null)
        {
            string unitId = soldier.data.unitName;
            
            soldier.animator.ResetState();
            
            UnitPool.Instance.ReturnToPool(gameObject, unitId);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRespawning()
    {
        isRespawning = true;
        Archer[] allArchers = FindObjectsOfType<Archer>();
        foreach (Archer archer in allArchers)
        {
            archer.ClearTargetIfMatches(this);
        }
    }

    public void FinishRespawning()
    {
        isRespawning = false;
    }
}