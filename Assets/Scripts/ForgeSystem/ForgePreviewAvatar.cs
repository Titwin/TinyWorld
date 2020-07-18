using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgePreviewAvatar : MonoBehaviour
{
    [Header("Avatar slots")]
    public HeadSlot head;
    public BodySlot body;
    public WeaponSlot weapon;
    public SecondSlot second;
    public ShieldSlot shield;
    public BackpackSlot backpack;

    [Header("Default apperance")]
    public HeadItem.Type defaultHead;
    public BodyItem.Type defaultBody;

    [Header("Linking")]
    private Animator animator;
    public AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;


    [Header("Configuration")]
    public float attackAnimationCooldown;
    private float attackAnimationTime;


    private void Start()
    {
        attackAnimationTime = 0f;

        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        AnimationParameterRefresh();
    }

    private void Update()
    {
        attackAnimationTime += Time.deltaTime;
        if(attackAnimationTime > attackAnimationCooldown)
        {
            animator.SetTrigger("attack");
            attackAnimationTime = 0f;
        }
    }

    public void AnimationParameterRefresh()
    {
        if (animator != null && clipOverrides != null)
        {
            // weapon code for attack animations
            if (weapon.equipedItem.type != WeaponItem.Type.None)
                animator.SetInteger("weapon", weapon.equipedItem.animationCode);
            else if (second.equipedItem.type != SecondItem.Type.None)
                animator.SetInteger("weapon", second.equipedItem.animationCode);
            else animator.SetInteger("weapon", 0);

            // shield for run, and idle
            animator.SetBool("shield", shield.equipedItem.type != ShieldItem.Type.None);
        
            // load animation clips
            AnimationClip[] clips = Arsenal.Instance.GetAnimationClip(ref weapon.equipedItem, ref second.equipedItem, ref shield.equipedItem, ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);

            clipOverrides["idle"] = clips[0];
            clipOverrides["walk"] = clips[1];
            clipOverrides["run"] = clips[2];
            clipOverrides["attack"] = clips[3];
            animatorOverrideController.ApplyOverrides(clipOverrides);
        }
    }

    public void AttackEnd() { }
}
