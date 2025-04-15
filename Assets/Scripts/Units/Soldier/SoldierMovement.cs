using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    private Soldier soldier;
    private GameBoardManager boardManager;
    private Pathfinder pathfinder;
    private Vector3 lastTarget = Vector3.positiveInfinity;
    private Vector3 lastPosition;
    private float movementCheckTimer, stuckCheckTimer, targetTrackTimer;
    private bool isMoving = false, isRetryingPath = false;
    private const float movementCheckDelay = 0.2f, stuckThreshold = 1f, targetOffsetRadius = 0.5f;
    private const float targetTrackDelay = 0.5f;

    public bool IsMoving => isMoving;

    public void Initialize(Soldier soldierRef, GameBoardManager boardManagerRef)
    {
        soldier = soldierRef;
        boardManager = boardManagerRef;
        pathfinder = new Pathfinder(boardManager);
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!isMoving) return;

        movementCheckTimer += Time.deltaTime;
        stuckCheckTimer += Time.deltaTime;
        targetTrackTimer += Time.deltaTime;

        if (movementCheckTimer < movementCheckDelay) return;
        movementCheckTimer = 0f;
        if (soldier.combat.TargetSoldier != null && targetTrackTimer >= targetTrackDelay)
        {
            targetTrackTimer = 0f;
            if (Vector3.Distance(transform.position, soldier.combat.TargetSoldier.transform.position) > 1.5f)
            {
                MoveTo(soldier.combat.TargetSoldier.transform.position);
            }
        }

        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
        {
            if (stuckCheckTimer >= stuckThreshold)
            {
                isMoving = false;
                soldier.animator.PlayIdleAnimation();
                CheckAttackOrRetryPath();
            }
        }
        else
        {
            lastPosition = transform.position;
            stuckCheckTimer = 0f;
            soldier.animator.PlayMoveAnimation();
        }
    }

    public void MoveTo(Vector3 target)
    {
        bool isBuildingTarget = soldier.combat.TargetBuilding && 
                                Vector3.Distance(target, soldier.combat.TargetBuilding.transform.position) < 1f;
                                
        bool isSoldierTarget = soldier.combat.TargetSoldier && 
                               Vector3.Distance(target, soldier.combat.TargetSoldier.transform.position) < 1f;

        Vector3 finalTarget;
        
        if (isSoldierTarget)
        {
            // For soldiers, we want to get closer than with buildings
            finalTarget = target;
        }
        else if (isBuildingTarget)
        {
            finalTarget = target;
        }
        else
        {
            finalTarget = CalculateOffset(target, soldier.unitIndex);
        }
        
        if (Vector3.Distance(finalTarget, lastTarget) < 0.1f && !isSoldierTarget) return;

        lastTarget = finalTarget;
        var path = pathfinder.FindPath(transform.position, finalTarget, transform.position.z);

        if (path != null && path.Count > 0)
        {
            StopAllCoroutines();
            if (isSoldierTarget)
            {
                StartCoroutine(FollowPath(path, null, soldier.combat.TargetSoldier));
            }
            else
            {
                StartCoroutine(FollowPath(path, soldier.combat.TargetBuilding));
            }
        }
        else if (isBuildingTarget)
        {
            TryAlternativePath(soldier.combat.TargetBuilding);
        }
        else if (isSoldierTarget)
        {
            isMoving = true;
            stuckCheckTimer = 0f;
            targetTrackTimer = targetTrackDelay; 
        }
        else
        {
            isMoving = false;
            soldier.animator.PlayIdleAnimation();
        }
    }

    private IEnumerator FollowPath(List<Vector3> path, BaseBuilding targetBuilding = null, Soldier targetSoldier = null)
    {
        isMoving = true;
        stuckCheckTimer = 0f;
        targetTrackTimer = 0f;
        soldier.animator.PlayMoveAnimation();

        float initialDist = 0f;
        if (targetBuilding != null)
            initialDist = Vector3.Distance(transform.position, targetBuilding.transform.position);
        else if (targetSoldier != null)
            initialDist = Vector3.Distance(transform.position, targetSoldier.transform.position);

        foreach (var point in path)
        {
            while (Vector3.Distance(transform.position, point) > 0.1f)
            {
                if (CheckAttackInterrupt()) yield break;

                // catch the target
                if (targetSoldier != null && Vector3.Distance(targetSoldier.transform.position, lastTarget) > 1.0f)
                {
                    MoveTo(targetSoldier.transform.position);
                    yield break;
                }

                transform.localScale = new Vector3(point.x > transform.position.x ? 1 : -1, 1, 1);
                float speed = soldier.data.moveSpeed > 0 ? soldier.data.moveSpeed : 3f;
                transform.position = Vector3.MoveTowards(transform.position, point, Time.deltaTime * speed);

                if (targetBuilding && soldier.combat.IsMovingToAttack)
                {
                    float dist = Vector3.Distance(transform.position, targetBuilding.transform.position);
                    float progress = 1f - (dist / initialDist);
                    if (progress >= 0.7f)
                    {
                        SetAttack();
                        yield break;
                    }
                }
                else if (targetSoldier && soldier.combat.IsMovingToAttack)
                {
                    float dist = Vector3.Distance(transform.position, targetSoldier.transform.position);
                    if (dist <= soldier.combat.soldierAttackRange)
                    {
                        SetAttack();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        isMoving = false;
        FinalizeMove(targetBuilding, targetSoldier);
    }

    private void FinalizeMove(BaseBuilding targetBuilding, Soldier targetSoldier = null)
    {
        if (targetBuilding == null && targetSoldier == null)
        {
            soldier.animator.PlayIdleAnimation();
            return;
        }

        if (targetBuilding != null)
        {
            if (soldier.combat.IsMovingToAttack)
            {
                float dist = Vector3.Distance(transform.position, targetBuilding.transform.position);
                if (dist <= 3f)
                {
                    SetAttack();
                }
                else
                {
                    TryAlternativePath(targetBuilding);
                }
            }
            else
            {
                if (soldier.combat.CheckIfInAttackRange(targetBuilding))
                    SetAttack();
                else
                    soldier.animator.PlayIdleAnimation();
            }
        }
        else if (targetSoldier != null)
        {
            if (soldier.combat.IsMovingToAttack)
            {
                float dist = Vector3.Distance(transform.position, targetSoldier.transform.position);
                if (dist <= soldier.combat.soldierAttackRange)
                {
                    SetAttack();
                }
                else
                {
                    MoveTo(targetSoldier.transform.position);
                }
            }
            else
            {
                if (soldier.combat.CheckIfInAttackRange(targetSoldier))
                    SetAttack();
                else
                    soldier.animator.PlayIdleAnimation();
            }
        }
    }

    private bool CheckAttackInterrupt()
    {
        //if we're attacking and not trying to move to attack
        if (soldier.combat.IsAttacking && !soldier.combat.IsMovingToAttack)
        {
            if (soldier.combat.TargetBuilding && 
                soldier.combat.CheckIfInAttackRange(soldier.combat.TargetBuilding))
                return true;
                
            if (soldier.combat.TargetSoldier && 
                soldier.combat.CheckIfInAttackRange(soldier.combat.TargetSoldier))
                return true;
        }
        
        return false;
    }

    private void CheckAttackOrRetryPath()
    {
        if (!soldier.combat.IsMovingToAttack) return;

        if (soldier.combat.TargetBuilding != null)
        {
            float dist = Vector3.Distance(transform.position, soldier.combat.TargetBuilding.transform.position);
            if (dist <= 3f)
            {
                SetAttack();
            }
            else
            {
                TryAlternativePath(soldier.combat.TargetBuilding);
            }
        }
        else if (soldier.combat.TargetSoldier != null)
        {
            float dist = Vector3.Distance(transform.position, soldier.combat.TargetSoldier.transform.position);
            if (dist <= 1.5f)
            {
                SetAttack();
            }
            else
            {
                MoveTo(soldier.combat.TargetSoldier.transform.position);
            }
        }
    }

    private void SetAttack()
    {
        soldier.combat.SetAttacking(true);
        soldier.animator.PlayAttackAnimation();
    }

    private void TryAlternativePath(BaseBuilding building)
    {
        if (isRetryingPath) return;
        isRetryingPath = true;

        var targets = GetAlternativeTargets(building);
        targets.Sort((a, b) => Vector3.Distance(transform.position, a).CompareTo(Vector3.Distance(transform.position, b)));

        foreach (var t in targets)
        {
            var path = pathfinder.FindPath(transform.position, t, transform.position.z);
            if (path != null && path.Count > 0)
            {
                StopAllCoroutines();
                StartCoroutine(FollowPath(path, building));
                isRetryingPath = false;
                return;
            }
        }

        isRetryingPath = false;

        float dist = Vector3.Distance(transform.position, building.transform.position);
        if (dist <= 5f) SetAttack();
        else soldier.combat.ClearTarget();
    }

    private List<Vector3> GetAlternativeTargets(BaseBuilding building)
    {
        List<Vector3> targets = new List<Vector3>();
        Vector3 center = building.transform.position;
        Collider2D col = building.GetComponent<Collider2D>();
        float radius = col ? Mathf.Max(col.bounds.extents.x, col.bounds.extents.y) + 0.5f : 2f;

        for (int i = 0; i < 16; i++)
        {
            float angle = (i / 16f) * Mathf.PI * 2;
            float dist = radius + (i / 16f) * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * dist;
            Vector3 point = center + offset;
            if (boardManager.IsWalkable(point)) targets.Add(point);
        }

        if (targets.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * Mathf.PI * 2;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (radius + 1f);
                targets.Add(center + offset);
            }
        }

        return targets;
    }

    private Vector3 CalculateOffset(Vector3 baseTarget, int index)
    {
        if (index == 0) return baseTarget;
        float angle = index * 137.5f;
        float dist = Mathf.Sqrt(index) * (targetOffsetRadius / 3f);
        return baseTarget + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * dist;
    }

    public void StopMovement()
    {
        isMoving = false;
        StopAllCoroutines();
    }
}