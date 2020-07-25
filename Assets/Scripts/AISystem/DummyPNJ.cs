using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPNJ : MonoBehaviour
{
    [Header("Movement and machine states")]
    public float aimingSpeed = 4;
    public float runSpeed = 4;
    public float gravity = 8;
    public bool activated = true;
    public Quaternion goal;

    [Header("Equipement")]
    public HorseSlot horse;
    public WeaponSlot weapon;
    public SecondSlot secondHand;
    public ShieldSlot shield;
    public BackpackSlot backpack;
    public HeadSlot head;
    public BodySlot body;
    public bool needEquipementAnimationUpdate = false;
    public AnimatorOverrideController animatorOverrideController;

    [Header("Animations")]
    public Transform target;
    public float threshold;
    public float time;
    private Animator animator;
    private AnimationClipOverrides clipOverrides;
    public GameObject pivot;
    private CharacterController controller;
    public float interactionCooldown;
    private float interactionTime;
    public TextMesh nameLabel;

    [Header("Discussions")]
    [TextArea(1,2)]
    public string pnjName;
    public string[] helloSentences;
    public List<DiscussionSubject> discussionSubjects = new List<DiscussionSubject>();
    public List<Quest> questList = new List<Quest>();

    public enum InterestAnimation
    {
        Idle,
        Interact
    }
    [System.Serializable]
    public struct InterestPoint
    {
        public string name;
        public Transform location;
        public float probability;
        public InterestAnimation animation;
        public float duration;
        public bool isHome;
        public AudioClip interactionSound;
    }
    [Header("Schedule")]
    public int interestIndex = 0;
    public List<InterestPoint> interestPoints;
    public bool moving = false;


    private AudioSource audiosource;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audiosource = GetComponent<AudioSource>();
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        pivot.SetActive(true);

        if (horse)
            horse.Equip(horse.equipedItem.type, true);
        weapon.Equip(weapon.equipedItem.type, true);
        secondHand.Equip(secondHand.equipedItem.type, true);
        head.Equip(head.equipedItem.type, true);
        shield.Equip(shield.equipedItem.type, true);

        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        body.Equip(body.equipedItem.type, mounted, true);
        backpack.Equip(backpack.equipedItem.type, true);

        AnimationParameterRefresh();
        animator.SetFloat("loadFactor", 1f);
        target = interestPoints[interestIndex].location;

        nameLabel.text = pnjName;
    }
    
    void Update()
    {
        if (!activated)
        {
            controller.Move(runSpeed * new Vector3(0, -gravity, 0) * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, aimingSpeed * Time.deltaTime);
            return;
        }

        Vector2 v = new Vector2(target.position.x, target.position.z) - new Vector2(transform.position.x, transform.position.z);
        if(v.magnitude < threshold)
        {
            animator.SetFloat("run", 0f);
            if (interestPoints[interestIndex].animation == InterestAnimation.Interact)
            {
                if(moving)
                    animator.SetTrigger("interact");
                animator.SetBool("interaction", true);
            }
            if (moving)
            {
                audiosource.clip = interestPoints[interestIndex].interactionSound;
                if (interestPoints[interestIndex].interactionSound && interestPoints[interestIndex].animation != InterestAnimation.Interact)
                    audiosource.Play();
            }

            controller.Move(runSpeed * new Vector3(0, -gravity, 0) * Time.deltaTime);
            goal = Quaternion.LookRotation(new Vector3(target.forward.x, 0, target.forward.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, aimingSpeed * Time.deltaTime);
            moving = false;
        }
        else
        {
            Vector3 direction = new Vector3(v.x, 0, v.y).normalized;
            direction.y = -gravity;

            if (direction.x == 0f && direction.z == 0f)
                animator.SetFloat("run", 0f);
            else animator.SetFloat("run", 1f);
            animator.SetBool("interaction", false);

            controller.Move(runSpeed * direction * Time.deltaTime);

            goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, aimingSpeed * Time.deltaTime);
            time = 0f;
            moving = true;
        }

        time += Time.deltaTime;
        if(time >= interestPoints[interestIndex].duration)
        {
            float value = Random.Range(0f, 1f);
            float sum = 0f;
            for (int i = 0; i < interestPoints.Count; i++) 
            {
                sum += interestPoints[i].probability;
                if (value < sum)
                {
                    target = interestPoints[i].location;
                    interestIndex = i;
                    break;
                }
            }
            time = 0f;
        }
    }
    public void SetActive(bool active)
    {
        activated = active;

        if (active)
        {
            moving = true;
            time = interestPoints[interestIndex].duration;
        }
        else
        {
            time = 0f;
            moving = false;
            animator.SetBool("interaction", false);
            animator.SetFloat("run", 0f);

            Vector3 v = PlayerController.MainInstance.transform.position - transform.position;
            goal = Quaternion.LookRotation(new Vector3(v.x, 0, v.z), Vector3.up);
        }
    }









    private void AnimationParameterRefresh()
    {
        // weapon code for attack animations
        if (weapon.equipedItem.type != WeaponItem.Type.None)
            animator.SetInteger("weapon", weapon.equipedItem.animationCode);
        else if (secondHand.equipedItem.type != SecondItem.Type.None)
            animator.SetInteger("weapon", secondHand.equipedItem.animationCode);
        else animator.SetInteger("weapon", 0);

        // shield for run, and idle
        animator.SetBool("shield", shield.equipedItem.type != ShieldItem.Type.None);
        
        // load animation clips
        AnimationClip[] clips;
        if (horse && horse.equipedItem.type != HorseItem.Type.None)
            clips = Arsenal.Instance.GetMountedAnimationClip(ref weapon.equipedItem, ref secondHand.equipedItem, ref shield.equipedItem, ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);
        else
            clips = Arsenal.Instance.GetAnimationClip(ref weapon.equipedItem, ref secondHand.equipedItem, ref shield.equipedItem, ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);

        clipOverrides["idle"] = clips[0];
        clipOverrides["walk"] = clips[1];
        clipOverrides["run"] = clips[2];
        clipOverrides["attack"] = clips[3];
        animatorOverrideController.ApplyOverrides(clipOverrides);

        needEquipementAnimationUpdate = false;
    }
    public void AttackEnd()
    {
        
    }
    public void InteractionTick()
    {
        audiosource.clip = interestPoints[interestIndex].interactionSound;
        if(audiosource.clip && interestPoints[interestIndex].animation == InterestAnimation.Interact)
            audiosource.Play();
    }
}
