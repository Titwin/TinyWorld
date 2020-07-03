using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement and machine states")]
    public float aimingSpeed = 4;
    public float aimingAttackSpeed = 20;
    public float runSpeed = 4;
    public float gravity = 8;
    public float attackCooldown = 0.8f;
    public bool attacking;
    public float loadFactor = 0f;
    public AnimationCurve loadCurve;
    private EventSystem eventsystem;

    private float grounded = 0f;
    private float attackDelay = 0f;
    private float offensiveSpearWeight = 0f;

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

    [Header("Interaction and juice")]
    public KeyCode interactKey = KeyCode.Space;
    public InteractionController interactionController;
    public Inventory inventory;
    public AudioClip effortSound;
    public AudioSource audiosource;
    public ParticleSystem particles;

    // Debug [Header("Debug")]
    private CharacterController controller;
    private Animator animator;
    private AnimationClipOverrides clipOverrides;
    private Vector3 direction = Vector3.zero;
    private Vector3 target;
    private ParticleSystem.EmitParams emitParams;

    #region Singleton
    public static PlayerController MainInstance { get; set; } = null;
    private void Awake()
    {
        if (MainInstance == null)
        {
            MainInstance = this;
        }
    }
    #endregion


    void Start()
    {
        eventsystem = (EventSystem)FindObjectOfType(typeof(EventSystem));
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        
        attacking = false;
        emitParams = new ParticleSystem.EmitParams();

        if(horse)
            horse.Equip(horse.equipedItem.type, true);
        weapon.Equip(weapon.equipedItem.type, true);
        secondHand.Equip(secondHand.equipedItem.type, true);
        head.Equip(head.equipedItem.type, true);
        shield.Equip(shield.equipedItem.type, true);

        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        body.Equip(body.equipedItem.type, mounted, true);
        backpack.Equip(backpack.equipedItem.type, true);

        AnimationParameterRefresh();
    }
    
    Vector3 GetInputDirection()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow))
            direction.z = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction.z = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction.x = 1;
        else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow))
            direction.x = -1;
        direction.Normalize();
        return direction;
    }
    void Update()
    {
        // begin
        if (ConstructionSystem.instance.activated)
        {
            InventoryUI.instance.Activate(false);
            return;
        }

        float speedFactor = Input.GetKey(KeyCode.LeftShift) && loadFactor > 0.75f ? 2 : 1;
        direction = Vector3.zero;

        // offensive posture on horse
        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        bool attack = mounted ? Input.GetMouseButtonUp(0) : Input.GetMouseButtonDown(0);
        attack &= !eventsystem.IsPointerOverGameObject();
        bool allowOffensivePosture = mounted && weapon.equipedItem.type != WeaponItem.Type.None && weapon.equipedItem.animationCode == 5;

        if (allowOffensivePosture && Input.GetMouseButton(0))
            offensiveSpearWeight = Mathf.Clamp(offensiveSpearWeight + 3 * Time.deltaTime, 0f, 1f);
        else if (allowOffensivePosture)
            offensiveSpearWeight = Mathf.Clamp(offensiveSpearWeight - 3 * Time.deltaTime, 0f, 1f);
        else offensiveSpearWeight = 0f;
        if(animator.GetLayerIndex("OffensivePosture") >= 0)
            animator.SetLayerWeight(animator.GetLayerIndex("OffensivePosture"), offensiveSpearWeight);
        
        // action or attack
        if (attack && attackDelay <= 0)
        {
            // if not click on UI
            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f, 1 << LayerMask.NameToLayer("PlayerUI")))
            {
                animator.SetTrigger("attack");
                attackDelay = attackCooldown;
                attacking = !mounted;
                audiosource.clip = effortSound;
                audiosource.Play();
                target = hit.point;
            }
        }

        // interaction
        else if(interactionController.hoveredInteractor && Input.GetKeyDown(interactKey) && !interactionController.interacting)
        {
            InteractionType[] interactions = interactionController.hoveredInteractor.GetComponents<InteractionType>();
            foreach (InteractionType interaction in interactions)
            {
                interactionController.Interact(interaction.type);
            }
        }

        // movement
        if ((controller.isGrounded || grounded < 0.2f) && !attacking && !interactionController.interacting)
        {
            // compute animation parameters
            direction = GetInputDirection();
            if (direction.x == 0f && direction.z == 0f)
                animator.SetFloat("run", 0f);
            else
                animator.SetFloat("run", speedFactor);
            animator.SetFloat("loadFactor", loadFactor * speedFactor);

            // update position
            direction = speedFactor * loadFactor * runSpeed * direction;
            direction = direction.x * Camera.main.transform.right + direction.z * Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            grounded = 0f;
        }
        else grounded += Time.deltaTime;

        // move
        direction.y = -gravity;
        controller.Move(direction * Time.deltaTime);

        // aiming
        if (attacking)
        {
            direction = (target - transform.position).normalized;
        }
        else if(interactionController.interacting && interactionController.hoveredInteractor != null)
        {
            direction = (interactionController.hoveredInteractor.transform.position - transform.position).normalized;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, (attacking | interactionController.interacting ? aimingAttackSpeed : aimingSpeed) * Time.deltaTime);
        }

        if ((direction.x != 0f || direction.z != 0f) && speedFactor >= 2 && Random.Range(0,4) == 0)
        {
            particles.Emit(emitParams, 1);
        }

        // update timers
        if (attackDelay > 0f)
            attackDelay -= Time.deltaTime;
        if (needEquipementAnimationUpdate)
            AnimationParameterRefresh();
    }

    
    private void AnimationParameterRefresh()
    {
        // weapon code for attack animations
        if (weapon.equipedItem.type != WeaponItem.Type.None)
            animator.SetInteger("weapon", weapon.equipedItem.animationCode);
        else if(secondHand.equipedItem.type != SecondItem.Type.None)
            animator.SetInteger("weapon", secondHand.equipedItem.animationCode);
        else animator.SetInteger("weapon", 0);

        // shield for run, and idle
        animator.SetBool("shield", shield.equipedItem.type != ShieldItem.Type.None);

        // compute load
        RecomputeLoadFactor();

        // load animation clips
        AnimationClip[] clips;
        if(horse && horse.equipedItem.type != HorseItem.Type.None)
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

    // Callbacks and coroutine
    public void AttackEnd()
    {
        attacking = false;
    }
    public void InteractionTick()
    {
        interactionController.InteractionTick();
        return;
    }

    // helper
    public void RecomputeLoadFactor()
    {
        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        float horseFactor = mounted ? 0.3f : 1f;
        if(backpack.equipedItem.type == BackpackItem.Type.RessourceContainer)
            backpack.equipedItem.load = 1f + 0.3f * inventory.RecomputeLoad();
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.07f * horseFactor * f);
        animator.SetFloat("loadFactor", loadFactor);
    }
    public static void Copy(PlayerController source, PlayerController destination)
    {
        // Equipement
        if(source.horse && destination.horse)
            destination.horse.Equip(source.horse.equipedItem.type, true);
        destination.weapon.Equip(source.weapon.equipedItem.type, true);
        destination.secondHand.Equip(source.secondHand.equipedItem.type, true);
        destination.shield.Equip(source.shield.equipedItem.type, true);
        destination.head.Equip(source.head.equipedItem.type, true);
        destination.body.Equip(source.body.equipedItem.type, true);
        destination.backpack.Equip(source.backpack.equipedItem.type, true);

        // other
        destination.transform.parent = source.transform.parent;
        destination.transform.position = source.transform.position;
        destination.transform.localScale = source.transform.localScale;
        destination.transform.rotation = source.transform.rotation;
    }

    public float GetLoad()
    {
        return loadFactor;
    }
    public float GetDammage()
    {
        return weapon.equipedItem.dammage + secondHand.equipedItem.dammage;
    }
    public float GetArmor()
    {
        return head.equipedItem.armor + body.equipedItem.armor + shield.equipedItem.armor + (horse ? horse.equipedItem.armor : 0);
    }
}


public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
