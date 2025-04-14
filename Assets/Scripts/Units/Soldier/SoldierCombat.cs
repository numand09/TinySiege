using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCombat : MonoBehaviour
{
    private Soldier soldier;
    private CapsuleCollider2D attackRangeCollider;
    
    private BaseBuilding targetBuilding;
    private float attackTimer;
    private bool isAttacking, isMovingToAttack;
    
    private readonly List<BaseBuilding> buildingsInRange = new();

    public BaseBuilding TargetBuilding => targetBuilding;
    public bool IsAttacking => isAttacking;
    public bool IsMovingToAttack => isMovingToAttack;

    public void Initialize(Soldier soldierRef)
    {
        soldier = soldierRef;
        attackRangeCollider = GetComponent<CapsuleCollider2D>();
        attackRangeCollider.isTrigger = true;
        attackRangeCollider.size = new Vector2(0.5f, 0.4f);
        attackRangeCollider.direction = CapsuleDirection2D.Horizontal;
        StartCoroutine(InitializeBuildingsInRange());
    }

    private void Update()
    {
        if (targetBuilding == null) return;

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

    private void TryAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= soldier.data.attackCooldown)
        {
            attackTimer = 0f;
            AttackBuilding(targetBuilding);
        }
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        transform.localScale = new Vector3(dir.x > 0 ? 1 : -1, 1, 1);
    }

    private IEnumerator InitializeBuildingsInRange()
    {
        yield return null;
        foreach (var col in Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f))
        {
            BaseBuilding building = col.GetComponent<BaseBuilding>();
            if (building && !buildingsInRange.Contains(building))
                buildingsInRange.Add(building);
        }
    }

    public bool CheckIfInAttackRange(BaseBuilding building)
    {
        foreach (var col in Physics2D.OverlapCapsuleAll(transform.position, attackRangeCollider.size, attackRangeCollider.direction, 0f))
            if (col.gameObject == building.gameObject) return true;

        return false;
    }

    public void SetTargetBuilding(BaseBuilding building)
    {
        targetBuilding = building;
        isMovingToAttack = true;
        isAttacking = false;
        MoveToTarget(building);
    }

    public void ClearTarget()
    {
        targetBuilding = null;
        isAttacking = isMovingToAttack = false;
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (attacking)
        {
            isMovingToAttack = false;
            if (targetBuilding) FaceTarget(targetBuilding.transform.position);
        }
    }

    private void MoveToTarget(BaseBuilding building)
    {
        Collider2D col = building.GetComponent<Collider2D>();
        Vector3 targetPos = col ? col.ClosestPoint(transform.position) : building.transform.position;
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
            MoveToTarget(building);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (!building) return;

        if (!buildingsInRange.Contains(building)) buildingsInRange.Add(building);

        if (building == targetBuilding && isMovingToAttack)
        {
            isAttacking = true;
            isMovingToAttack = false;
            soldier.movement.StopMovement();
            soldier.animator.PlayAttackAnimation();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseBuilding building = other.GetComponent<BaseBuilding>();
        if (building) buildingsInRange.Remove(building);
    }
}
