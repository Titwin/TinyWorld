using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IDamageable
{
    void GetDamage(int amount);
    int GetTeam();
}
public class CharacterAnimationController : MonoBehaviour
{
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
    public Unit character;

    [Header("Movement and machine states")]
    public float aimingSpeed = 4;
    public float aimingAttackSpeed = 20;
    public float runSpeed = 4;
    public float gravity = 8;
    public float attackCooldown = 0.8f;
    public bool attacking;
    public float loadFactor = 0f;
    public AnimationCurve loadCurve;

    [Header("Equipement slots")]
    public HorseSlot horse;
    public WeaponSlot weapon;
    public SecondSlot secondHand;
    public ShieldSlot shield;
    public BackpackSlot backpack;
    public HeadSlot head;
    public BodySlot body;

    [Header("Debug")]
    public Transform interactionBox;
    public Transform attackBox;

    [Header("Links")]
    [SerializeField] public Rigidbody rb;
    [SerializeField] new Collider collider;
    [SerializeField] private Animator animator;
    [SerializeField] protected AnimatorOverrideController animatorOverrideController;
    [SerializeField] protected AnimationClipOverrides clipOverrides;
    [SerializeField] private ParticleSystem runParticles = null;
    [SerializeField] private ParticleSystem bloodParticles = null;
    [SerializeField] private ParticleSystem.EmitParams emitParams;

    [SerializeField] bool equipOnStart = true;

    [Header("STATUS")]
    [SerializeField] private Vector3 direction = Vector3.zero;
    [SerializeField] bool grounded;
    [SerializeField] private float attackDelay = 0f;
    [SerializeField] float speedFactor = 1;
    [SerializeField] AgentBase toAttack = null;
    [SerializeField] bool interactButton = false;
    [SerializeField] bool runButton = false;
    [SerializeField] public bool die = false;
    [SerializeField] WeaponItem.Type currentEquipment;
    [SerializeField] bool hurt = false;

    int HASH_ATTACK;
    int HASH_RUN;
    int HASH_LOADFACTOR;
    int HASH_WEAPON;
    int HASH_SHIELD;
    int HASH_DMG;
    int HASH_DEATH;
    int HASH_DEATH_IDX;


    public float cooldownRatio { get { return attackDelay > 0 ? attackDelay / attackCooldown : 0; } }

    private void Awake()
    {
        HASH_ATTACK = Animator.StringToHash("attack");
        HASH_RUN = Animator.StringToHash("run");
        HASH_LOADFACTOR = Animator.StringToHash("loadFactor");
        HASH_WEAPON = Animator.StringToHash("weapon");
        HASH_SHIELD = Animator.StringToHash("shield");
        HASH_DMG = Animator.StringToHash("damage");
        HASH_DEATH = Animator.StringToHash("die");
        HASH_DEATH_IDX = Animator.StringToHash("dead");

        character.StateChanged += OnStateChanged;
        character.OnDamageReceived += OnDamageReceived;

       
    }

    void OnStateChanged()
    {
        if(!die && character.State == Unit.UnitState.Dead)
        {
            die = true;
            animator.SetInteger(HASH_DEATH_IDX, Random.Range(0, 2));
            animator.SetTrigger(HASH_DEATH);
            bloodParticles.Emit(50);
            character.State = Unit.UnitState.Dead;
            StartCoroutine(Banish());
        }
    }
    void OnDamageReceived()
    {
        hurt = true;
    }

    private void OnValidate()
    {
        animator = GetComponent<Animator>();
        if (!runParticles) runParticles = GetComponent<ParticleSystem>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!collider) collider = GetComponent<Collider>();
    }
    void Start()
    {
        OnValidate();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        emitParams = new ParticleSystem.EmitParams();

        if (equipOnStart)
        {
            weapon.Equip(character.mainWeapon, true);
            secondHand.Equip(character.SecondaryWeapon, true);
            head.Equip(character.head, true);
            shield.Equip(character.shield, true);
            body.Equip(character.body, true);
            backpack.Equip(character.backpack, true);
        }
        attacking = false;
        AnimationParameterRefresh();
    }


    public void SetInput(Vector3 _direction, bool run = false, AgentBase attack = null, bool interact = false)
    {
        direction = _direction;
        this.runButton = run;
        this.interactButton = interact;

        if (attack && toAttack != attack)
        {
            {
                var newEquipment = currentEquipment;
                var source = attack as ResourceSource;
                if (source)
                {
                    if (source.resource == ResourceSource.ResourceType.Wood)
                    {
                        newEquipment = WeaponItem.Type.AxeA;
                    }
                    else if (source.resource == ResourceSource.ResourceType.Stone)
                    {
                        newEquipment = WeaponItem.Type.Pickaxe;
                    }
                    else if (source.resource == ResourceSource.ResourceType.Gold)
                    {
                        newEquipment = WeaponItem.Type.Pickaxe;
                    }
                    else if (source.resource == ResourceSource.ResourceType.Food)
                    {
                        newEquipment = WeaponItem.Type.None;
                    }
                }
                else
                {
                    var building = attack as BuildingBase;
                    if (building && building.team == this.GetComponent<AgentBase>().team)
                    {
                        newEquipment = WeaponItem.Type.Hammer;
                    }
                    else
                    {
                        newEquipment = character.mainWeapon;
                    }
                }

                if (newEquipment != currentEquipment)
                {
                    weapon.Equip(newEquipment, false);
                    currentEquipment = newEquipment;
                    AnimationParameterRefresh();
                }
            }
        }
        this.toAttack = attack;
    }

    [SerializeField] Transform projectileEmitter = null;

    public void Tick()
    {
        float deltaTime = Time.deltaTime;

        animator.SetBool(HASH_ATTACK, false);
        
        if (!character.Alive)
        {
            return;
        }
        /*else if (hurt)
        {
            animator.SetTrigger(HASH_DMG);
            hurt = false;
           
        }*/
        else
        {
            if (hurt)
            {
                hurt = false;
                bloodParticles.Emit(20);
            }

            if (attackDelay > 0)
            {
                attackDelay -= deltaTime;
                if (attackDelay <= 0)
                {
                    attacking = false;
                }
            }

            grounded = true;
            if (!grounded)
            {
                // move 
                direction.y = -gravity * deltaTime;
                character.State = Unit.UnitState.Falling;
            }
            // attack trigger
            else if (toAttack && attackDelay <= 0)
            {
                character.State = Unit.UnitState.Attacking_windup;
                animator.SetTrigger(HASH_ATTACK);
                attackDelay = attackCooldown;
                attacking = true;
            }

            else if (attacking)
            {
                if (character.State == Unit.UnitState.Attacking_windup && attackDelay < attackCooldown / 2)
                {
                    if (character.weapon.projectilePrefab)
                    {
                        Projectile pinstance = Instantiate<Projectile>(character.weapon.projectilePrefab);
                        pinstance.transform.parent = null;
                        pinstance.transform.position = projectileEmitter.transform.position;
                        pinstance.transform.rotation = projectileEmitter.transform.rotation;
                        pinstance.team = character.team;
                        pinstance.transform.parent = null;
                        pinstance.gameObject.SetActive(true);
                        pinstance.rb.velocity = pinstance.transform.up * 30;
                    }
                    else
                    {
                        if (toAttack)
                        {
                            var enemy = toAttack as Unit;
                            if (enemy)
                            {
                                character.DoAttack(enemy);
                            }
                            else
                            {
                                /*var building = toAttack as Building;
                                if (building)
                                {
                                    if (building.team == this.GetComponent<Actor>().team)
                                    {
                                        character.DoWorkOn(building);
                                    }
                                    else
                                    {
                                        character.DoAttack(building);
                                    }
                                }
                                else
                                {
                                    var source = toAttack as ResourceSource;
                                    if (source)
                                    {
                                        character.DoWorkOn(source);
                                    }
                                }*/
                            }
                        }
                    }
                    character.State = Unit.UnitState.Attacking_relase;
                }
            }
            // moving
            else
            {
                speedFactor = runButton ? 2 : 1;
                // compute animation parameters
                if (direction.x == 0f && direction.z == 0f)
                {
                    animator.SetFloat(HASH_RUN, 0f);
                    character.State = Unit.UnitState.Idle;
                }
                else
                {
                    animator.SetFloat(HASH_RUN, speedFactor);
                    animator.SetFloat(HASH_LOADFACTOR, loadFactor * speedFactor);

                    if ((direction.x != 0f || direction.z != 0f) && runButton && Random.Range(0, 4) == 0)
                    {
                        runParticles.Emit(emitParams, 1);
                        character.State = Unit.UnitState.Running;
                    }
                    else
                    {
                        character.State = Unit.UnitState.Walking;
                    }
                }
            }
        }
    }

    private bool Equip(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        if (type == InteractionType.Type.pickableWeapon)
        {
            WeaponItem item = interactor.GetComponent<WeaponItem>();
            if (item && weapon.Equip(item.type))
            {
                if (item.forbidSecond || secondHand.equipedItem.forbidWeapon)
                    secondHand.Equip(SecondItem.Type.None);
                if (item.forbidShield)
                    shield.Equip(ShieldItem.Type.None);
                success = true;
            }
        }
        else if (type == InteractionType.Type.pickableBackpack)
        {
            BackpackItem item = interactor.GetComponent<BackpackItem>();
            if (item && backpack.Equip(item.type))
                success = true;
        }
        else if (type == InteractionType.Type.pickableHead)
        {
            HeadItem item = interactor.GetComponent<HeadItem>();
            if (item && head.Equip(item.type))
                success = true;
        }
        else if (type == InteractionType.Type.pickableSecond)
        {
            SecondItem item = interactor.GetComponent<SecondItem>();
            if (item && secondHand.Equip(item.type))
            {
                if (item.forbidWeapon || weapon.equipedItem.forbidSecond)
                    weapon.Equip(WeaponItem.Type.None);
                if (item.forbidShield)
                    shield.Equip(ShieldItem.Type.None);
                success = true;
            }
        }
        else if (type == InteractionType.Type.pickableShield)
        {
            ShieldItem item = interactor.GetComponent<ShieldItem>();
            if (item && shield.Equip(item.type))
            {
                if (weapon.equipedItem.forbidShield)
                    weapon.Equip(WeaponItem.Type.None);
                if (secondHand.equipedItem.forbidShield)
                    secondHand.Equip(SecondItem.Type.None);
                success = true;
            }
        }
        else if (type == InteractionType.Type.pickableBody)
        {
            bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
            BodyItem item = interactor.GetComponent<BodyItem>();
            if (item && body.Equip(item.type, mounted))
                success = true;
        }
        else
        {
            Debug.LogWarning("no interaction defined for this type " + type.ToString());
            return false;
        }
        return success;
    }

    private void AnimationParameterRefresh()
    {
        // weapon code for attack animations
        if (weapon.equipedItem.type != WeaponItem.Type.None)
            animator.SetInteger(HASH_WEAPON, weapon.equipedItem.animationCode);
        else if (secondHand.equipedItem.type != SecondItem.Type.None)
            animator.SetInteger(HASH_WEAPON, secondHand.equipedItem.animationCode);
        else animator.SetInteger(HASH_WEAPON, 0);

        // shield for run, and idle
        animator.SetBool(HASH_SHIELD, shield.equipedItem.type != ShieldItem.Type.None);

        // compute load
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.1f * f);
        animator.SetFloat(HASH_LOADFACTOR, loadFactor);

        // load clips
        AnimationClip[] clips = Arsenal.Instance.GetAnimationClip(ref weapon.equipedItem, ref secondHand.equipedItem, ref shield.equipedItem, ref body.equipedItem, ref head.equipedItem, ref backpack.equipedItem);
        clipOverrides["idle"] = clips[0];
        clipOverrides["walk"] = clips[1];
        clipOverrides["run"] = clips[2];
        clipOverrides["attack"] = clips[3];
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public void AttackEnd()
    {
        attacking = false;
    }

    IEnumerator Banish()
    {
        rb.isKinematic = true;
        collider.enabled = false;
        Vector3 position = this.transform.position;
        yield return new WaitForSeconds(2);
        animator.enabled = false;
        for(float t = 0; t < 2; t += Time.deltaTime)
        {
            yield return new WaitForEndOfFrame();
            this.transform.position = Vector3.Lerp(position, position - Vector3.up * 1, t / 2f);
        }
        character.State = Unit.UnitState.Free;
    }
}
