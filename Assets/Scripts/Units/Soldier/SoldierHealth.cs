using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Health management component
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
        // Don't take damage while respawning
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

    {   soldier.selection.rangeIndicator.SetActive(false);  
        soldier.animator.PlayDeathAnimation();
        StartCoroutine(DeathAndDestroy());
    }

    private IEnumerator DeathAndDestroy()
    {
        // Wait for animation to complete without any fade effect
        yield return new WaitForSeconds(1.5f);
        
        // Return to UnitPool instead of using GameObjectPool
        if (UnitPool.Instance != null)
        {
            string unitId = soldier.data.unitName;
            UnitPool.Instance.ReturnToPool(gameObject, unitId);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when soldier is about to respawn
    public void StartRespawning()
    {
        isRespawning = true;
        // Find all archers and reset their target if they're targeting this soldier
        Archer[] allArchers = FindObjectsOfType<Archer>();
        foreach (Archer archer in allArchers)
        {
            archer.ClearTargetIfMatches(this);
        }
    }

    // Call this when soldier has fully respawned
    public void FinishRespawning()
    {
        isRespawning = false;
    }
}