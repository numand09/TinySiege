using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCombat : MonoBehaviour
{
    private Soldier soldier;
    private CapsuleCollider2D attackRangeCollider;
    public float soldierAttackRange = 0.5f;

    
    private BaseBuilding targetBuilding;
    private Soldier targetSoldier;
    private float attackTimer;
    private bool isAttacking, isMovingToAttack;
    
    private readonly List<BaseBuilding> buildingsInRange = new();
    private readonly List<Soldier> soldiersInRange = new();

    public BaseBuilding TargetBuilding => targetBuilding;
    public Soldier TargetSoldier => targetSoldier;
    public bool IsAttacking => isAttacking;
    public bool IsMovingToAttack => isMovingToAttack;
    public bool HasTarget => targetBuilding != null || targetSoldier != null;

    public void Initialize(Soldier soldierRef)
    {
        soldier = soldierRef;
        attackRangeCollider = GetComponent<CapsuleCollider2D>();
        attackRangeCollider.isTrigger = true;
        attackRangeCollider.size = new Vector2(0.5f, 0.4f);
        attackRangeCollider.direction = CapsuleDirection2D.Horizontal;
        StartCoroutine(InitializeTargetsInRange());
    }

    private void Update()
    {
        if (targetBuilding == null && targetSoldier == null) return;

        if (targetBuilding != null)
        {
            HandleBuildingTarget();
            return;
        }

        if (targetSoldier != null)
        {
            HandleSoldierTarget();
        }
    }

    private void HandleBuildingTarget()
    {
        if (targetBuilding.currentHP <= 0)
        {
            ClearTarget();
            soldier.animator.PlayIdleAnimation();
            return;
        }

        FaceTarget(targetBuilding.transform.position);

        if (CheckIfInAttackRange(targetBuilding))
        {
            if (isAttacking || isMovingToAttack)
            {
                isMovingToAttack = false;
                isAttacking = true;
                soldier.movement.StopMovement();
                soldier.animator.PlayAttackAnimation();
                TryAttack();
            }
        }
        else if (isAttacking)
        {
            TryAttack();
        }
    }

    private void HandleSoldierTarget()
    {
        if (targetSoldier.health.currentHP <= 0 || targetSoldier.health.isRespawning)
        {
            ClearTarget();
            soldier.animator.PlayIdleAnimation();
            return;
        }

        FaceTarget(targetSoldier.transform.position);

        if (CheckIfInAttackRange(targetSoldier))
        {
            if (isAttacking || isMovingToAttack)
            {
                isMovingToAttack = false;
                isAttacking = true;
                soldier.movement.StopMovement();
                soldier.animator.PlayAttackAnimation();
                TryAttack();
            }
        }
        else
        {
            isAttacking = false;
            isMovingToAttack = true;
            MoveTo(targetSoldier.transform.position);
        }
    }

    private void TryAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= soldier.data.attackCooldown)
        {
            attackTimer = 0f;
            
            if (targetBuilding != null)
            {
                AttackBuilding(targetBuilding);
            }
            else if (targetSoldier != null)
            {
                AttackSoldier(targetSoldier);
            }
        }
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        transform.localScale = new Vector3(dir.x > 0 ? 1 : -1, 1, 1);
    }

    private IEnumerator InitializeTargetsInRange()
    {
        yield return null;
        foreach (var col in Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f))
        {
            BaseBuilding building = col.GetComponent<BaseBuilding>();
            if (building && !buildingsInRange.Contains(building))
                buildingsInRange.Add(building);
                
            Soldier enemySoldier = col.GetComponent<Soldier>();
            if (enemySoldier && enemySoldier != soldier && !soldiersInRange.Contains(enemySoldier))
                soldiersInRange.Add(enemySoldier);
        }
    }

    public bool CheckIfInAttackRange(BaseBuilding building)
    {
        foreach (var col in Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f))
            if (col.gameObject == building.gameObject) return true;

        return false;
    }
    
    public bool CheckIfInAttackRange(Soldier target)
    {
        if (target == null) return false;
        
        return Vector3.Distance(transform.position, target.transform.position) <= soldierAttackRange;
    }

    public void SetTargetBuilding(BaseBuilding building)
    {
        targetBuilding = building;
        targetSoldier = null;
        isMovingToAttack = true;
        isAttacking = false;
        MoveTo(building.transform.position);
    }
    
    public void SetTargetSoldier(Soldier target)
    {
        // Don't target self
        if (target == soldier) return;
        
        targetSoldier = target;
        targetBuilding = null;
        isMovingToAttack = true;
        isAttacking = false;
        MoveTo(target.transform.position);
    }

    public void ClearTarget()
    {
        targetBuilding = null;
        targetSoldier = null;
        isAttacking = isMovingToAttack = false;
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (attacking)
        {
            isMovingToAttack = false;
            if (targetBuilding) 
                FaceTarget(targetBuilding.transform.position);
            else if (targetSoldier)
                FaceTarget(targetSoldier.transform.position);
        }
    }

    private void MoveTo(Vector3 targetPos)
    {
        soldier.movement.MoveTo(targetPos);
    }

    private void AttackBuilding(BaseBuilding building)
    {
        if (Vector3.Distance(transform.position, building.transform.position) <= 2f)
        {
            soldier.animator.PlayAttackAnimation();
            building.TakeDamage(soldier.data.damage);
            if (building.currentHP <= 0) building.DestroyBuilding();
        }
        else
        {
            isAttacking = false;
            isMovingToAttack = true;
            MoveTo(building.transform.position);
        }
    }
    
    private void AttackSoldier(Soldier target)
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= soldierAttackRange)
        {
            soldier.animator.PlayAttackAnimation();
            target.health.TakeDamage(soldier.data.damage);
        }
        else
        {
            isAttacking = false;
            isMovingToAttack = true;
            MoveTo(target.transform.position);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (building)
        {
            if (!buildingsInRange.Contains(building)) buildingsInRange.Add(building);

            if (building == targetBuilding && isMovingToAttack)
            {
                isAttacking = true;
                isMovingToAttack = false;
                soldier.movement.StopMovement();
                soldier.animator.PlayAttackAnimation();
            }
        }
        
        Soldier otherSoldier = other.GetComponent<Soldier>();
        if (otherSoldier && otherSoldier != soldier)
        {
            if (!soldiersInRange.Contains(otherSoldier)) soldiersInRange.Add(otherSoldier);
            
            if (otherSoldier == targetSoldier && isMovingToAttack)
            {
                isAttacking = true;
                isMovingToAttack = false;
                soldier.movement.StopMovement();
                soldier.animator.PlayAttackAnimation();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (building) buildingsInRange.Remove(building);
        
        Soldier otherSoldier = other.GetComponent<Soldier>();
        if (otherSoldier) soldiersInRange.Remove(otherSoldier);
    }
}