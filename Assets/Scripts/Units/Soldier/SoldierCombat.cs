using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCombat : MonoBehaviour
{
    private Soldier soldier;
    private CapsuleCollider2D attackRangeCollider;
    
    private BaseBuilding targetBuilding;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    
    private List<BaseBuilding> buildingsInRange = new List<BaseBuilding>();

    public BaseBuilding TargetBuilding => targetBuilding;
    public bool IsAttacking => isAttacking;

    public void Initialize(Soldier soldierRef)
    {
        soldier = soldierRef;
        
        // Setup attack range collider
        attackRangeCollider = GetComponent<CapsuleCollider2D>();
        if (attackRangeCollider == null)
        {
            attackRangeCollider = gameObject.AddComponent<CapsuleCollider2D>();
        }
        attackRangeCollider.isTrigger = true;
        attackRangeCollider.size = new Vector2(1f, 0.3f);
        attackRangeCollider.direction = CapsuleDirection2D.Horizontal;
        
        // Initialize buildings in range
        StartCoroutine(InitializeBuildingsInRange());
    }

    private void Update()
    {
        if (targetBuilding != null)
        {
            // If we have a target building, check if it's still valid
            if (targetBuilding.currentHP <= 0)
            {
                targetBuilding = null;
                isAttacking = false;
                soldier.animator.PlayIdleAnimation();
                return;
            }
            
            // Check if we're in range
            bool inRange = CheckIfInAttackRange(targetBuilding);
                        
            // Only attack if we're in range and in attacking state
            if (isAttacking && inRange)
            {
                // Face the target
                Vector3 direction = targetBuilding.transform.position - transform.position;
                transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
                
                attackTimer += Time.deltaTime;
                if (attackTimer >= soldier.data.attackCooldown)
                {
                    attackTimer = 0f;
                    AttackBuilding(targetBuilding);
                }
            }
            else if (!inRange && isAttacking)
            {
                // We're out of range, need to move closer
                MoveToTarget(targetBuilding);
            }
        }
    }

    private IEnumerator InitializeBuildingsInRange()
    {
        // Wait a frame to ensure everything is initialized
        yield return null;
        
        // Find all colliders in our attack range using OverlapCapsule
        Collider2D[] colliders = Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f);
        
        foreach (Collider2D col in colliders)
        {
            BaseBuilding building = col.GetComponent<BaseBuilding>();
            if (building != null && !buildingsInRange.Contains(building))
            {
                buildingsInRange.Add(building);
            }
        }
    }

    public bool CheckIfInAttackRange(BaseBuilding building)
    {
        // Check the capsule for colliders
        Collider2D[] colliders = Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f);
        
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == building.gameObject)
            {
                return true;
            }
        }
        
        return false;
    }

    public void SetTargetBuilding(BaseBuilding building)
    {
        targetBuilding = building;
        
        // Check if already in range
        if (CheckIfInAttackRange(building))
        {
            // We're already in range, start attacking
            isAttacking = true;
            soldier.movement.StopMovement();
            soldier.animator.PlayAttackAnimation();
            return;
        }
        
        // We need to move closer
        MoveToTarget(building);
    }

    public void ClearTarget()
    {
        targetBuilding = null;
        isAttacking = false;
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    private void MoveToTarget(BaseBuilding building)
    {
        // Calculate a position within attack range of the building
        Vector3 dirToBuilding = (transform.position - building.transform.position).normalized;
        if (dirToBuilding.magnitude < 0.001f)
        {
            // If we're at the same position, pick a random direction
            dirToBuilding = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        }
        
        // Calculate slightly inside attack range to ensure we're in range
        float moveToDistance = attackRangeCollider.size.x * 0.8f;
        Vector3 targetPosition = building.transform.position + dirToBuilding * moveToDistance;
        
        // Move to that position
        soldier.movement.MoveTo(targetPosition);
        
        // Start attacking when in range
        if (targetBuilding == building) {
            StartCoroutine(AttackWhenInRange());
        }
    }

    private IEnumerator AttackWhenInRange()
    {
        // Wait a small amount to allow movement to start
        yield return new WaitForSeconds(0.2f);
        
        while (targetBuilding != null)
        {
            // Check if we're in range
            bool inRange = CheckIfInAttackRange(targetBuilding);
            
            if (inRange)
            {
                isAttacking = true;
                soldier.movement.StopMovement();
                soldier.animator.PlayAttackAnimation();
                yield break;
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

private void AttackBuilding(BaseBuilding building)
{
    soldier.animator.PlayAttackAnimation();
    building.TakeDamage(soldier.data.damage);

    if (building.currentHP <= 0)
    {
        building.DestroyBuilding(); // Havuz sistemine geri döner
    }
}


    // Track buildings in range
    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (building != null)
        {
            if (!buildingsInRange.Contains(building))
            {
                buildingsInRange.Add(building);
            }
            
            // Only attack if this building is our target and we've explicitly been ordered to attack
            if (building == targetBuilding && isAttacking)
            {
                soldier.movement.StopMovement();
                StopAllCoroutines(); // Stop any movement
                soldier.animator.PlayAttackAnimation();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (building != null && buildingsInRange.Contains(building))
        {
            buildingsInRange.Remove(building);
        }
    }
}
