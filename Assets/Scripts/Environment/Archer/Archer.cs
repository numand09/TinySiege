using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour
{
    [Header("Archer Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int arrowDamage = 1;
    [SerializeField] private Transform bowPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private int poolSize = 3;
    [SerializeField] private float arrowSpeed = 8f;
    [SerializeField] private float arrowLifetime = 5f;
    
    private Transform arrowPoolParent;

    private SoldierHealth currentTarget;
    private float attackTimer = 0f;
    private bool canAttack = true;
    private List<GameObject> arrowPool;
    private GameObject activeArrow;
    private LayerMask soldierLayer;
    [SerializeField] private Transform poolsParent;


    private void Start()
    {
        soldierLayer = LayerMask.GetMask("Soldier");
        CreatePoolParent();
        InitializeArrowPool();
        SpawnBowArrow();
    }
    
    private void CreatePoolParent()
    {
        GameObject poolParentObj = new GameObject("ArrowPool");
        arrowPoolParent = poolParentObj.transform;
        arrowPoolParent.SetParent(poolsParent);
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        
        if (currentTarget == null || !IsTargetInRange(currentTarget) || !IsTargetValid(currentTarget))
            FindNewTarget();
        
        if (currentTarget != null && attackTimer >= attackCooldown && canAttack && IsTargetValid(currentTarget))
        {
            FaceTarget(currentTarget.transform.position);
            ShootArrow();
            attackTimer = 0f;
        }
    }

    private bool IsTargetValid(SoldierHealth target)
    {
        return target != null && target.currentHP > 0 && !target.isRespawning;
    }

    public void ClearTargetIfMatches(SoldierHealth soldierHealth)
    {
        if (currentTarget == soldierHealth)
        {
            currentTarget = null;
        }
    }

    private void InitializeArrowPool()
    {
        arrowPool = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
            arrowPool.Add(CreateArrowInstance());
    }

    private GameObject CreateArrowInstance()
    {
        GameObject arrow = Instantiate(arrowPrefab);
        Arrow arrowComponent = arrow.GetComponent<Arrow>() ?? arrow.AddComponent<Arrow>();
        
        arrowComponent.Initialize(this, arrowDamage, arrowSpeed, arrowLifetime);
        arrow.SetActive(false);
        arrow.transform.SetParent(arrowPoolParent);
        
        return arrow;
    }

    private GameObject GetArrowFromPool()
    {
        foreach (GameObject arrow in arrowPool)
            if (!arrow.activeInHierarchy)
                return arrow;
        
        GameObject newArrow = CreateArrowInstance();
        arrowPool.Add(newArrow);
        return newArrow;
    }

    private void FindNewTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange, soldierLayer);
        
        float closestDistance = float.MaxValue;
        SoldierHealth closestSoldier = null;
        
        foreach (Collider2D col in hitColliders)
        {
            SoldierHealth soldier = col.GetComponent<SoldierHealth>();
            if (soldier != null && soldier.currentHP > 0 && !soldier.isRespawning)
            {
                float distance = Vector2.Distance(transform.position, soldier.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSoldier = soldier;
                }
            }
        }
        
        currentTarget = closestSoldier;
    }

    private bool IsTargetInRange(SoldierHealth target)
    {
        if (target == null || target.currentHP <= 0 || target.isRespawning)
            return false;
            
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

    private void ShootArrow()
    {
        if (currentTarget == null || currentTarget.isRespawning)
            return;
                    
        GameObject arrow = GetArrowFromPool();
        arrow.transform.position = bowPoint.position;
        arrow.SetActive(true);
        
        arrow.transform.SetParent(null);
        
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        arrowComponent.LaunchToTarget(currentTarget.transform);
        
        StartCoroutine(RespawnBowArrow(0.2f));
        
        if (activeArrow != null)
            activeArrow.SetActive(false);
    }

    private void SpawnBowArrow()
    {
        if (activeArrow == null)
        {
            activeArrow = Instantiate(arrowPrefab, bowPoint);
            activeArrow.transform.localPosition = Vector3.zero;
            activeArrow.transform.localRotation = Quaternion.identity;
            
            DisableArrowComponents(activeArrow);
        }
        else
        {
            activeArrow.SetActive(true);
        }
    }
    
    private void DisableArrowComponents(GameObject arrow)
    {
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent != null)
            arrowComponent.enabled = false;
        
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;
        
        Collider2D col = arrow.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    private IEnumerator RespawnBowArrow(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnBowArrow();
    }
    
    private void OnDestroy()
    {
        if (arrowPoolParent != null)
        {
            Destroy(arrowPoolParent.gameObject);
        }
    }
    
    public void ResetArcherState()
    {
        attackTimer = attackCooldown;
        currentTarget = null;
        if (activeArrow == null)
        {
            SpawnBowArrow();
        }
        else
        {
            activeArrow.SetActive(true);
        }
        
        canAttack = true;
        FindNewTarget();
    }
}