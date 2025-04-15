using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimator : MonoBehaviour
{
    private Soldier soldier;
    private Animator animator;
    public AnimationClip idleClip, moveClip, attackClip, deathClip;
    
    // Ölüm animasyonu için kontrol değişkenleri
    private bool isDying = false;
    
    public void Initialize(Soldier soldierRef)
    {
        soldier = soldierRef;
        animator = GetComponentInChildren<Animator>();
        PlayIdleAnimation();
    }
    
    public void PlayIdleAnimation()
    {
        if (isDying) return;
        
        if (animator != null && idleClip != null)
        {
            animator.Play(idleClip.name);
        }
    }
    
    public void PlayMoveAnimation()
    {
        if (isDying) return;
        
        if (animator != null && moveClip != null)
        {
            animator.Play(moveClip.name);
        }
    }
    
    public void PlayAttackAnimation()
    {
        if (isDying) return;
        
        if (animator != null && attackClip != null)
        {
            animator.Play(attackClip.name);
        }
    }
    
    public void PlayDeathAnimation()
    {
        if (animator != null && deathClip != null)
        {
            isDying = true;
            animator.Play(deathClip.name, 0, 0f);           
            StartCoroutine(EnsureDeathAnimation());
        }
    }
    
    private IEnumerator EnsureDeathAnimation()
    {
        yield return null;
        
        float animTime = 0f;
        float maxAnimTime = deathClip != null ? deathClip.length : 1.5f;
        
        while (animTime < maxAnimTime)
        {
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(deathClip.name))
            {
                animator.Play(deathClip.name);
            }
            
            animTime += Time.deltaTime;
            yield return null;
        }
    }
    
    public bool IsDying()
    {
        return isDying;
    }
    
    public void ResetState()
    {
        isDying = false;
    }
}