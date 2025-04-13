using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    private Soldier soldier;
    private GameBoardManager boardManager;
    private Pathfinder pathfinder;
    
    private Vector3 lastPosition;
    private Vector3 lastTarget = Vector3.positiveInfinity;
    private bool isMoving = false;
    private float movementCheckTimer = 0f;
    private float movementCheckDelay = 0.2f;
    
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
        // Check if we're moving
        if (isMoving)
        {
            movementCheckTimer += Time.deltaTime;
            if (movementCheckTimer >= movementCheckDelay)
            {
                movementCheckTimer = 0f;
                if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
                {
                    // We're not actually moving despite being in movement state
                    isMoving = false;
                    soldier.animator.PlayIdleAnimation();
                }
                else
                {
                    // We're still moving
                    soldier.animator.PlayMoveAnimation();
                    lastPosition = transform.position;
                }
            }
        }
    }

    public void MoveTo(Vector3 target)
    {
        if (Vector3.Distance(target, lastTarget) < 0.1f) return;
        lastTarget = target;
    
        var path = pathfinder.FindPath(transform.position, target, transform.position.z);
    
        if (path != null && path.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(FollowPath(path));
        }
        else
        {
            isMoving = false;
            soldier.animator.PlayIdleAnimation();
        }
    }

    private IEnumerator FollowPath(List<Vector3> path)
    {
        isMoving = true;
        soldier.animator.PlayMoveAnimation();
        
        foreach (var point in path)
        {
            while (Vector3.Distance(transform.position, point) > 0.1f)
            {
                if (soldier.combat.IsAttacking && soldier.combat.TargetBuilding != null && 
                    soldier.combat.CheckIfInAttackRange(soldier.combat.TargetBuilding))
                {
                    isMoving = false;
                    yield break;
                }
                
                transform.localScale = new Vector3(point.x > transform.position.x ? 1 : -1, 1, 1);
                float moveSpeed = soldier.data.moveSpeed > 0 ? soldier.data.moveSpeed : 3f;
                transform.position = Vector3.MoveTowards(transform.position, point, Time.deltaTime * moveSpeed);
                yield return null;
            }
        }
        
        isMoving = false;
        
        // Check if we've reached our target for attacking
        if (soldier.combat.TargetBuilding != null)
        {
            bool inRange = soldier.combat.CheckIfInAttackRange(soldier.combat.TargetBuilding);
            if (inRange)
            {
                soldier.combat.SetAttacking(true);
                soldier.animator.PlayAttackAnimation();
            }
            else
            {
                soldier.animator.PlayIdleAnimation();
            }
        }
        else
        {
            soldier.animator.PlayIdleAnimation();
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        StopAllCoroutines();
    }
}