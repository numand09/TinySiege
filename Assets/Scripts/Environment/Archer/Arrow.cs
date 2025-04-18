using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Archer owner;
    private int damage;
    private float speed;
    private float lifetime;
    private bool isInitialized;
    private Transform target;
    private float timer;
    private Rigidbody2D rb;
    private Transform poolParent;
    
    private Vector3 originalScale;
    private float minScaleFactor = 0.4f;
    private float shrinkingSpeed = 2.5f;

    private void Awake()
    {
        SetupComponents();
        originalScale = transform.localScale;
        
        if (transform.parent != null && transform.parent.name == "ArrowPool")
        {
            poolParent = transform.parent;
        }
    }

    private void SetupComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        if (GetComponent<Collider2D>() == null)
        {
            CapsuleCollider2D col = gameObject.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.1f, 0.1f);
            col.direction = CapsuleDirection2D.Horizontal;
            col.isTrigger = true;
        }
    }

    public void Initialize(Archer archer, int arrowDamage, float arrowSpeed, float arrowLifetime)
    {
        owner = archer;
        damage = arrowDamage;
        speed = arrowSpeed;
        lifetime = arrowLifetime;
        isInitialized = true;
        
        transform.localScale = originalScale;
    }

    public void LaunchToTarget(Transform targetTransform)
    {
        if (!isInitialized)
            return;
        
        target = targetTransform;
        timer = 0f;
        transform.localScale = originalScale;
        
        if (rb != null)
            rb.simulated = true;
        
        StartCoroutine(MoveToTarget());
    }

    private float EaseOutQuad(float t)
    {
        return t * (2 - t);
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, target.position);
        
        while (gameObject.activeInHierarchy)
        {
            timer += Time.deltaTime;
            
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                ReturnToPool();
                yield break;
            }
            
            MoveArrowTowardsTarget(startPosition, journeyLength);
            
            if (timer >= lifetime)
            {
                ReturnToPool();
                yield break;
            }
            
            yield return null;
        }
    }
    
    private void MoveArrowTowardsTarget(Vector3 startPosition, float journeyLength)
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        ApplyPerspectiveEffect(startPosition, journeyLength);
    }
    
    private void ApplyPerspectiveEffect(Vector3 startPosition, float journeyLength)
    {
        float distanceCovered = Vector3.Distance(transform.position, startPosition);
        float journeyFraction = distanceCovered / journeyLength;
        
        float easedFraction = Mathf.Clamp01(EaseOutQuad(journeyFraction * shrinkingSpeed));
        float currentScale = Mathf.Lerp(1.0f, minScaleFactor, easedFraction);
        
        transform.localScale = originalScale * currentScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SoldierHealth soldier = other.GetComponent<SoldierHealth>();
        if (soldier != null)
        {
            if (!soldier.isRespawning)
            {
                soldier.TakeDamage(damage);
            }
            
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        StopAllCoroutines();
        
        if (rb != null)
            rb.simulated = false;
        
        transform.localScale = originalScale;
        
        GameObject arrowPool = GameObject.Find("ArrowPool");
        if (arrowPool != null)
        {
            transform.SetParent(arrowPool.transform);
        }
        
        gameObject.SetActive(false);
    }
}