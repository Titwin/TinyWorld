using System.Collections;
using System.Collections.Generic;
using UnityEngine;




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

    [Header("Interaction")]
    public bool interacting;
    public float collectCooldown = 0.8f;
    public float pickCooldown = 2f;
    public InteractionType.Type interactionType;
    public GameObject currentInteractor = null;
    public InteractionJuicer interactionJuicer;
    public ResourceContainer ressourceContainer;
    public InventoryViewer inventoryViewer;
    public EquipementViewer equipementViewer;
    public InteractionHelper interactionHelper;

    private bool needEquipementAnimationUpdate = false;
    public  float interactionDelay = 0f;
    public float interactionConfirmedDelay = 0f;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private int scanLength;
    private Dictionary<string, string> interactionConditionList = new Dictionary<string, string>();

    [Header("Sounds")]
    public List<AudioClip> wearHead;
    public List<AudioClip> wearBody;
    public List<AudioClip> movementHeavy;
    public AudioClip backpackClear;
    public AudioClip effortSound;
    private AudioSource audiosource;
    
    [Header("Debug")]
    public Transform interactionBox;

    private CharacterController controller;
    private Animator animator;
    public AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
    private ParticleSystem particles;
    private Vector3 direction = Vector3.zero;
    private Vector3 target;
    private ParticleSystem.EmitParams emitParams;

    public static PlayerController MainInstance { get; set; } = null;
    private void Awake()
    {
        if (MainInstance == null)
        {
            MainInstance = this;
        }
    }


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorOverrideController;
        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        
        particles = GetComponent<ParticleSystem>();
        attacking = false;
        interacting = false;
        emitParams = new ParticleSystem.EmitParams();
        audiosource = GetComponent<AudioSource>();

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

        ressourceContainer.AddItem("Stone", 8);
        ressourceContainer.AddItem("Wood", 3);
        ressourceContainer.AddItem("Iron", 1);

        /*ressourceContainer.AddItem("Gold", 100);
        ressourceContainer.AddItem("Iron", 100);
        ressourceContainer.AddItem("Stone", 300);
        ressourceContainer.AddItem("Wood", 300);
        ressourceContainer.AddItem("Wheat", 100);
        ressourceContainer.AddItem("Crystal", 10);*/
        if (inventoryViewer.visible)
            inventoryViewer.UpdateContent(ressourceContainer.inventory);
        inventoryViewer.transform.parent = null;
        equipementViewer.transform.parent = null;
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
            return;

        float speedFactor = Input.GetKey(KeyCode.LeftShift) && loadFactor > 0.75f ? 2 : 1;
        direction = Vector3.zero;

        // offensive posture on horse
        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        bool attack = mounted ? Input.GetMouseButtonUp(0) : Input.GetMouseButtonDown(0);
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
        else if(scanLength != 0 && Input.GetKeyDown(KeyCode.Space) && !interacting)
        {
            GameObject go = scanResults[0].collider.gameObject;
            InteractionType[] interactions = go.GetComponents<InteractionType>();
            foreach (InteractionType interaction in interactions)
            {
                Interact(interaction.type, go);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Space) && scanLength == 0 && weapon.equipedItem.toolFamily == "Hammer" && !interacting)
        {
            interactionType = InteractionType.Type.constructionMode;
            currentInteractor = ConstructionSystem.instance.gameObject;
            interactionConfirmedDelay = 0.001f;
        }
        interacting = currentInteractor && (interactionDelay > 0f || interactionConfirmedDelay > 0f);
        animator.SetBool("interaction", interacting);

        // interaction time integral
        if (interacting && Input.GetKey(KeyCode.Space) && interactionConfirmedDelay > 0f)
        {
            interactionConfirmedDelay += Time.deltaTime;
            if (interactionConfirmedDelay >= pickCooldown)
            {
                InteractionConfirmed();
                interactionConfirmedDelay = 0f;
            }
        }
        else interactionConfirmedDelay = 0f;
        interactionJuicer.loadingRate = interactionConfirmedDelay / pickCooldown;

        // movement
        if ((controller.isGrounded || grounded < 0.2f) && !attacking && !interacting)
        {
            // compute direction
            direction = GetInputDirection();

            // compute animation parameters
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
        else if(interacting)
        {
            direction = (currentInteractor.transform.position - transform.position).normalized;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, (attacking | interacting ? aimingAttackSpeed : aimingSpeed) * Time.deltaTime);
        }

        if ((direction.x != 0f || direction.z != 0f) && speedFactor >= 2 && Random.Range(0,4) == 0)
        {
            particles.Emit(emitParams, 1);
        }

        // update timers
        if (attackDelay > 0f)
            attackDelay -= Time.deltaTime;
        if (interactionDelay > 0f && !Input.GetKey(KeyCode.Space))
            interactionDelay -= Time.deltaTime;
        
        if (needEquipementAnimationUpdate)
            AnimationParameterRefresh();
    }
    private void LateUpdate()
    {
        Vector3 size = new Vector3(controller.radius * 1.1f, 0.5f * controller.height, controller.radius * 1.1f);
        Vector3 position = transform.TransformPoint(controller.center);
        scanLength = Physics.BoxCastNonAlloc(position, size, Vector3.forward, scanResults, Quaternion.identity, 1f, 1 << LayerMask.NameToLayer("Interaction"));

        if (scanLength > 0 && !ConstructionSystem.instance.activated)
            interactionJuicer.hovered = scanResults[0].collider.gameObject;
        else interactionJuicer.hovered = null;
        if (interactionJuicer.hovered != null && interactionJuicer.hovered.GetComponent<ResourceContainer>() != null)
            inventoryViewer.storage = interactionJuicer.hovered.GetComponent<ResourceContainer>();
        else inventoryViewer.storage = null;

        interactionBox.position = position;
        interactionBox.localScale = 2*size;
    }

    private bool Interact(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        
        switch(type)
        {
            // pick an item
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
            case InteractionType.Type.pickableHorse:
                interactionType = type;
                currentInteractor = interactor;
                success = true;
                interactionConfirmedDelay = 0.001f;
                break;

            case InteractionType.Type.storeRessources:
                if (ressourceContainer.load != 0)
                {
                    interactionType = type;
                    currentInteractor = interactor;
                    success = true;
                    interactionConfirmedDelay = 0.001f;
                }
                else
                {
                    interactionConditionList.Clear();
                    interactionConditionList.Add("Container", "empty");
                    interactionHelper.UpdateContent(interactionConditionList);
                    success = false;
                }
                break;

            // standart ressources collection
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
            case InteractionType.Type.collectWheet:
            case InteractionType.Type.construction:
            case InteractionType.Type.destroyBuilding:
                success = InitiateInteractionTick(type, interactor);
                break;
                
            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + type.ToString());
                success = false;
                break;
        }
        return success;
    }
    private void InteractionConfirmed()
    {
        switch (interactionType)
        {
            // pick an item
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
            case InteractionType.Type.pickableHorse:
                pickableInteraction(interactionType, currentInteractor);
                break;

            // standart ressources collection -> no confirmed action
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
                break;

            // store all ressources
            case InteractionType.Type.storeRessources:
                storeAllInteraction(interactionType, currentInteractor);
                break;

            case InteractionType.Type.constructionMode:
                ConstructionSystem.instance.SetActive(true);
                break;

            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + interactionType.ToString());
                break;
        }
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
    private bool InitiateInteractionTick(InteractionType.Type type, GameObject interactor)
    {
        bool success = true;
        ComputeInteractionConditions(type);
        foreach(KeyValuePair<string, string> entry in interactionConditionList)
        {
            if(entry.Value != "ok")
            {
                success = false;
                break;
            }
        }
        
        if(!success)
            interactionHelper.UpdateContent(interactionConditionList);
        else
        {
            interactionDelay = collectCooldown;
            animator.SetTrigger("interact");
            currentInteractor = interactor;
            interactionType = type;
        }
        return success;
    }
    private bool pickableInteraction(InteractionType.Type type, GameObject interactor)
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
                needEquipementAnimationUpdate = true;

                if(ToolDictionary.Instance.toolTypes.ContainsKey(weapon.equipedItem.toolFamily))
                {
                    List<AudioClip> sounds = ToolDictionary.Instance.Get(weapon.equipedItem.toolFamily).collectionSound;
                    audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                    audiosource.Play();
                }
            }
        }
        else if (type == InteractionType.Type.pickableBackpack)
        {
            BackpackItem item = interactor.GetComponent<BackpackItem>();
            if (item && backpack.Equip(item.type))
                success = true;
            needEquipementAnimationUpdate = true;
            ressourceContainer.Clear();

            if (ToolDictionary.Instance.toolTypes.ContainsKey(backpack.equipedItem.toolFamily))
            {
                List<AudioClip> sounds = ToolDictionary.Instance.Get(backpack.equipedItem.toolFamily).collectionSound;
                audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableHead)
        {
            HeadItem item = interactor.GetComponent<HeadItem>();
            if (item && head.Equip(item.type))
            {
                success = true;
                needEquipementAnimationUpdate = true;
                int index = Mathf.Clamp((int)HeadItem.getCategory(head.equipedItem.type), 0, wearHead.Count - 1);
                audiosource.clip = wearHead[index];
                audiosource.Play();
            }
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
                needEquipementAnimationUpdate = true;
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
                needEquipementAnimationUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableBody)
        {
            BodyItem item = interactor.GetComponent<BodyItem>();
            bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
            if (item && body.Equip(item.type, mounted))
            {
                success = true;
                needEquipementAnimationUpdate = true;
                int index = Mathf.Clamp((int)BodyItem.getCategory(body.equipedItem.type), 0, wearBody.Count - 1);
                audiosource.clip = wearBody[index];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableHorse)
        {
            HorseItem item = interactor.GetComponent<HorseItem>();
            if((horse && item.type == HorseItem.Type.None) ||(!horse && item.type != HorseItem.Type.None))
            {
                // change player template
                PlayerController destination;
                if (item.type == HorseItem.Type.None)
                    destination = Instantiate(Arsenal.Instance.playerTemplate.gameObject).GetComponent<PlayerController>();
                else destination = Instantiate(Arsenal.Instance.mountedPlayerTemplate.gameObject).GetComponent<PlayerController>();
                PlayerController.Copy(this, destination);
                PlayerController.MainInstance = destination;
                MapStreaming.instance.focusAgent = destination.transform;

                Destroy(gameObject);
                Destroy(inventoryViewer.gameObject);
                Destroy(equipementViewer.gameObject);

                destination.pickableInteraction(type, interactor);
                //constructionCamera.gameObject.GetComponent<TPSCameraController>().target = destination.transform.Find("CameraTarget");
            }
            else
            {
                if (horse && item && horse.Equip(item.type))
                {
                    success = true;
                    needEquipementAnimationUpdate = true;
                    /*int index = Mathf.Clamp((int)BodyItem.getCategory(body.equipedItem.type), 0, wearBody.Count - 1);
                    audiosource.clip = wearBody[index];
                    audiosource.Play();*/

                    bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
                    body.Equip(body.equipedItem.type, mounted, true);
                }
            }
        }

        if (success)
            equipementViewer.UpdateContent();

        return success;
    }
    private bool storeAllInteraction(InteractionType.Type type, GameObject interactor)
    {
        ResourceContainer storehouse = interactor.GetComponent<ResourceContainer>();
        if(storehouse)
        {
            Dictionary<string, int> transfers = new Dictionary<string, int>();
            Dictionary<string, int> accepted = storehouse.GetAcceptance();
            foreach (KeyValuePair<string, int> entry in ressourceContainer.inventory)
            {
                if (accepted.Count == 0 || accepted.ContainsKey(entry.Key))
                {
                    int currentCount = storehouse.inventory.ContainsKey(entry.Key) ? storehouse.inventory[entry.Key] : 0;
                    int maxCount = (accepted.ContainsKey(entry.Key) && accepted[entry.Key] > 0) ? accepted[entry.Key] : storehouse.capacity;
                    
                    if (storehouse.HasSpace() && maxCount != currentCount)
                    {
                        int maximumTransfert = Mathf.Min(entry.Value, maxCount - currentCount);
                        storehouse.AddItem(ResourceDictionary.Instance.Get(entry.Key).name, maximumTransfert);
                        transfers.Add(entry.Key, maximumTransfert);
                    }
                }
            }

            if (transfers.Count != 0)
            {
                foreach(KeyValuePair<string, int> entry in transfers)
                    ressourceContainer.RemoveItem(entry.Key, entry.Value, false);
                ressourceContainer.UpdateContent();
                if (inventoryViewer.visible)
                {
                    inventoryViewer.UpdateContent(inventoryViewer.GetFusionInventory());
                }
                RecomputeLoadFactor();
                audiosource.clip = backpackClear;
                audiosource.Play();
            }
            else
            {
                if (ressourceContainer.inventory.Count == 0)
                {
                    Debug.LogWarning("Impossible to be here, pb in container load computing");
                }
                else
                {
                    interactionConditionList.Clear();
                    foreach (string acceptance in storehouse.acceptedResources)
                    {
                        string accName = acceptance;
                        if(acceptance.Contains(" "))
                        {
                            char[] separator = { ' ' };
                            accName = acceptance.Split(separator)[0];
                        }
                        interactionConditionList.Add(accName, "nor");
                    }
                    interactionHelper.UpdateContent(interactionConditionList);
                }
            }
        }
        return true;
    }

    // Callbacks and coroutine
    public void AttackEnd()
    {
        attacking = false;
    }
    public void InteractionTick()
    {
        interactionDelay = collectCooldown;
        if (currentInteractor && interactionType == InteractionType.Type.collectWood)
        {
            CommonRessourceCollectionResolve();
            interactionJuicer.treeInteractor = currentInteractor;
        }
        else if (InteractionType.isCollectingMinerals(interactionType))
        {
            CommonRessourceCollectionResolve();
        }
        else if (currentInteractor && interactionType == InteractionType.Type.collectWheet)
        {
            CommonRessourceCollectionResolve();
        }
        else if (currentInteractor && interactionType == InteractionType.Type.construction)
        {
            // hammer on stone is close to pickake on stone for Iron collection
            List<AudioClip> sounds = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(InteractionType.Type.collectIron)).collectionSound;
            AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
            audiosource.clip = soundFx;
            if (soundFx)
                audiosource.Play();

            // increment progress bar
            ConstructionController construction = currentInteractor.GetComponent<ConstructionController>();
            if (construction && construction.Increment())
            {
                interactionDelay = 0f;
                currentInteractor = null;
            }
        }
        else if (currentInteractor && interactionType == InteractionType.Type.destroyBuilding)
        {
            // hammer on stone is close to pickake on stone for Iron collection
            List<AudioClip> sounds = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(InteractionType.Type.collectIron)).collectionSound;
            AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
            audiosource.clip = soundFx;
            if (soundFx)
                audiosource.Play();

            // increment progress bar
            DestructionTemplate destruction = currentInteractor.GetComponent<DestructionTemplate>();
            if (destruction.Increment())
            {
                interactionDelay = 0f;
                currentInteractor = null;
            }
        }
        else
        {
            interacting = false;
            currentInteractor = null;
            interactionDelay = 0f;
            animator.SetBool("interaction", false);
            Debug.LogWarning("no interaction tick for this type implemented");
        }
    }
    private void CommonRessourceCollectionResolve()
    {
        // play sound, and juice, and update inventory
        List<AudioClip> sounds = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(interactionType)).collectionSound;
        AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
        audiosource.clip = soundFx;
        if (soundFx)
            audiosource.Play();
        int gain = Random.Range(1, 4);
        interactionJuicer.treeInteractor = currentInteractor;
        interactionJuicer.LaunchGainAnim("+" + gain.ToString(), interactionType);

        // update inventory and compute load
        ressourceContainer.AddItem(ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(interactionType)).name, gain);
        RecomputeLoadFactor();
        if (inventoryViewer.visible)
        { 
            inventoryViewer.UpdateContent(ressourceContainer.inventory);
        }

        // decrement interactor ressource count
        CollectData data = currentInteractor.GetComponent<CollectData>();
        data.ressourceCount--;
        if (data.ressourceCount <= 0)
        {
            Destroy(currentInteractor.transform.parent.gameObject);
            interactionDelay = 0;
            interacting = false;
            currentInteractor = null;
        }

        // stop interaction loop if needed
        if(!ressourceContainer.HasSpace())
        {
            ComputeInteractionConditions(interactionType);
            interactionHelper.UpdateContent(interactionConditionList);
            interactionDelay = 0;
            interacting = false;
            animator.SetBool("interaction", false);
        }
    }
    private void ComputeInteractionConditions(InteractionType.Type type)
    {
        interactionConditionList.Clear();
        if(type == InteractionType.Type.construction || type == InteractionType.Type.forge || type == InteractionType.Type.destroyBuilding)
        {
            interactionConditionList.Add("Hammer", weapon.equipedItem.toolFamily == "Hammer" ? "ok" : "nok");
        }
        else // try whith standart ressource collection
        {
            string[] equiped = { backpack.equipedItem.toolFamily , weapon.equipedItem.toolFamily };
            ResourceType resource = ResourceDictionary.Instance.Get(ResourceDictionary.Instance.GetNameFromType(type));
            foreach (string tool in resource.collectionTools)
            {
                string status = "nok";
                foreach (string s in equiped)
                {
                    if(s == tool)
                    {
                        if (tool == "Container")
                            status = ressourceContainer.HasSpace() ? "ok" : "full";
                        else status = "ok";
                    }
                }
                interactionConditionList.Add(tool, status);
            }
        }

    }

    // helper
    public void RecomputeLoadFactor()
    {
        bool mounted = horse ? horse.equipedItem.type != HorseItem.Type.None : false;
        float horseFactor = mounted ? 0.3f : 1f;
        if(backpack.equipedItem.type == BackpackItem.Type.RessourceContainer)
            backpack.equipedItem.load = 1f + 0.3f * ressourceContainer.RecomputeLoad();
        float f = body.equipedItem.load + weapon.equipedItem.load + secondHand.equipedItem.load + shield.equipedItem.load + head.equipedItem.load + backpack.equipedItem.load;
        loadFactor = loadCurve.Evaluate(0.07f * horseFactor * f);
        animator.SetFloat("loadFactor", loadFactor);
        equipementViewer.UpdateContent();
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
        ResourceContainer.Copy(source.ressourceContainer, destination.ressourceContainer);

        // other
        destination.transform.parent = source.transform.parent;
        destination.transform.position = source.transform.position;
        destination.transform.localScale = source.transform.localScale;
        destination.transform.rotation = source.transform.rotation;
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
