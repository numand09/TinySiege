using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimator : MonoBehaviour
{
    private Soldier soldier;
    private Animator animator;
    
    public AnimationClip idleClip, moveClip, attackClip, deathClip;

    public void Initialize(Soldier soldierRef)
    {
        soldier = soldierRef;
        animator = GetComponentInChildren<Animator>();
        PlayIdleAnimation();
    }

    public void PlayIdleAnimation()
    {
        if (animator != null && idleClip != null)
        {
            animator.Play(idleClip.name);
        }
    }
    
    public void PlayMoveAnimation()
    {
        if (animator != null && moveClip != null)
        {
            animator.Play(moveClip.name);
        }
    }
    
    public void PlayAttackAnimation()
    {
        if (animator != null && attackClip != null)
        {
            animator.Play(attackClip.name);
        }
    }
    
    public void PlayDeathAnimation()
    {
        if (animator != null && deathClip != null)
        {
            animator.Play(deathClip.name);
        }
    }
}
