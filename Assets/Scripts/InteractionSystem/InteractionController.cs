﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("Linking")]
    public CharacterController characterController;
    public PlayerController playerController;
    public ResourceContainer resourceContainer;
    public InteractionJuicer interactionJuicer;
    public AudioSource audiosource;
    public Animator animator;
    public KeyCode interactKey;

    [Header("Research")]
    public GameObject hoveredInteractor;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private int scanLength = 0;

    [Header("Timer and state")]
    public bool interacting;
    public InteractionType.Type lastInteraction;
    public float delayedInteractionDuration;
    private float delayedInteractionTime;
    public float interactionCooldown;
    private float interactionTime;

    [Header("Help")]
    public float helpSpacing;
    public float helpDuration = 1f;
    public GameObject helpContainer;
    public InteractionConditionTemplate template;
    public Sprite valid;
    public Sprite invalid;
    private IEnumerator helpTimerCoroutine;
    private Dictionary<string, string> interactionConditionList = new Dictionary<string, string>();

    [Header("Sounds")]
    public AudioClip errorSound;
    public AudioClip backpackClear;
    public List<AudioClip> wearHead;
    public List<AudioClip> wearBody;
    public List<AudioClip> movementHeavy;

    // debug
    private Vector3 scanPosition = Vector3.zero;
    private Vector3 scanSize = Vector3.one;

    
    void Start()
    {
        if (!characterController)
            characterController = GetComponent<CharacterController>();
        if (!audiosource)
            audiosource = GetComponent<AudioSource>();
        if (!playerController)
            playerController = GetComponent<PlayerController>();

        interactionTime = 0;
        StopTimer();
    }
    
    void Update()
    {
        interacting = (hoveredInteractor != null) && (delayedInteractionTime > 0f || interactionTime > 0f);
        animator.SetBool("interaction", interacting);

        if (interacting && Input.GetKey(interactKey) && delayedInteractionTime > 0f)
        {
            delayedInteractionTime += Time.deltaTime;
            if (delayedInteractionTime >= delayedInteractionDuration)
            {
                InteractionConfirmed();
                StopTimer();
            }
        }
        else StopTimer();

        if (interactionTime > 0f && !Input.GetKey(interactKey))
            interactionTime -= Time.deltaTime;
        interactionJuicer.loadingRate = delayedInteractionTime / delayedInteractionDuration;
    }
    private void LateUpdate()
    {
        if (!characterController)
            return;

        Vector3 size = new Vector3(characterController.radius * 0.6f, 0.5f * characterController.height, characterController.radius * 0.6f);
        Vector3 position = playerController.transform.TransformPoint(characterController.center);
        scanLength = Physics.BoxCastNonAlloc(position, size, Vector3.up, scanResults, Quaternion.identity, 1f, 1 << LayerMask.NameToLayer("Interaction"));

        if (scanLength > 0 && !ConstructionSystem.instance.activated)
            hoveredInteractor = scanResults[0].collider.gameObject;
        else hoveredInteractor = null;

        interactionJuicer.hovered = hoveredInteractor;

        // debug
        scanPosition = position;
        scanSize = 2 * size;
    }



    public bool Interact(InteractionType.Type type)
    {
        bool success = false;

        switch (type)
        {
            // using confimation system
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
            case InteractionType.Type.pickableHorse:
                lastInteraction = type;
                success = true;
                StartTimer();
                break;

            case InteractionType.Type.storeRessources:
                if (resourceContainer.load != 0)
                {
                    lastInteraction = type;
                    success = true;
                    StartTimer();
                }
                else
                {
                    interactionConditionList.Clear();
                    interactionConditionList.Add("Container", "empty");
                    UpdateHelp();
                    success = false;
                }
                break;

            // using interaction tick system
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
            case InteractionType.Type.collectWheat:
            case InteractionType.Type.construction:
            case InteractionType.Type.destroyBuilding:
                success = InitiateInteractionTick(type);
                break;

            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + type.ToString());
                success = false;
                break;
        }
        return success;
    }
    public void InteractionConfirmed()
    {
        switch (lastInteraction)
        {
            // pick an item
            case InteractionType.Type.pickableBackpack:
            case InteractionType.Type.pickableBody:
            case InteractionType.Type.pickableHead:
            case InteractionType.Type.pickableSecond:
            case InteractionType.Type.pickableShield:
            case InteractionType.Type.pickableWeapon:
            case InteractionType.Type.pickableHorse:
                PickableInteraction(lastInteraction, hoveredInteractor);
                break;

            // store all ressources at once
            case InteractionType.Type.storeRessources:
                StoreAllInteraction(lastInteraction, hoveredInteractor);
                break;

            case InteractionType.Type.constructionMode:
                ConstructionSystem.instance.SetActive(true);
                break;

            // standard ressources collection -> no confirmed action (and if we are here it's an error)
            case InteractionType.Type.collectStone:
            case InteractionType.Type.collectIron:
            case InteractionType.Type.collectGold:
            case InteractionType.Type.collectCrystal:
            case InteractionType.Type.collectWood:
                break;

            // error
            default:
                Debug.LogWarning("no interaction defined for this type " + lastInteraction.ToString());
                break;
        }
    }
    public bool InitiateInteractionTick(InteractionType.Type type)
    {
        // check if conditions are Ok
        bool success = true;
        ComputeInteractionConditions(type);
        foreach (KeyValuePair<string, string> entry in interactionConditionList)
        {
            if (entry.Value != "ok")
            {
                success = false;
                break;
            }
        }

        // do action or show help
        if (!success)
            UpdateHelp();
        else
        {
            interactionTime = interactionCooldown;
            animator.SetTrigger("interact");
            lastInteraction = type;
        }
        return success;
    }
    public void StartTimer()
    {
        // start timer for confirmation interaction
        if (hoveredInteractor == null)
            hoveredInteractor = gameObject;
        delayedInteractionTime = 0.001f;
    }
    public void StopTimer()
    {
        // stop timer for confirmation interaction
        delayedInteractionTime = 0f;
    }
    public void UpdateHelp()
    {
        // initialize
        if (helpContainer.activeSelf && helpTimerCoroutine != null)
            StopCoroutine(helpTimerCoroutine);
        foreach (Transform child in helpContainer.transform)
            Destroy(child.gameObject);

        // create items to show and ple them
        Vector3 position = new Vector3(-0.5f * helpSpacing * (interactionConditionList.Count - 1), 0, 0);
        foreach (KeyValuePair<string, string> entry in interactionConditionList)
        {
            InteractionConditionTemplate go = Instantiate(template, helpContainer.transform);
            go.transform.localPosition = position;
            go.gameObject.SetActive(true);

            if (ToolDictionary.instance.tools.ContainsKey(entry.Key))
                go.mainIcon.sprite = ToolDictionary.instance.tools[entry.Key].icon;
            else if (ResourceDictionary.instance.resources.ContainsKey(entry.Key))
                go.mainIcon.sprite = ResourceDictionary.instance.resources[entry.Key].icon;
            else
                Debug.Log("no tool icon for this entry : " + entry.Key);

            if (entry.Value == "ok")
            {
                go.validationIcon.sprite = valid;
                go.specialText.gameObject.SetActive(false);
                go.validationIcon.gameObject.SetActive(true);
            }
            else if (entry.Value == "nok")
            {
                go.validationIcon.sprite = invalid;
                go.specialText.gameObject.SetActive(false);
                go.validationIcon.gameObject.SetActive(true);
            }
            else
            {
                go.specialText.text = entry.Value;
                go.specialText.gameObject.SetActive(true);
                go.validationIcon.gameObject.SetActive(false);
            }
            position.x += helpSpacing;
        }

        // start showing and start a timer for hiding
        helpTimerCoroutine = HelpTimer(helpDuration);
        StartCoroutine(helpTimerCoroutine);
    }
    public void InteractionTick()
    {
        // reset cooldown
        interactionTime = interactionCooldown;

        if(hoveredInteractor)
        {
            if(lastInteraction == InteractionType.Type.collectWood)
            {
                interactionJuicer.treeInteractor = hoveredInteractor;
                CommonRessourceCollectionResolve();
            }
            else if (InteractionType.isCollectingMinerals(lastInteraction))
            {
                CommonRessourceCollectionResolve();
            }
            else if (lastInteraction == InteractionType.Type.collectWheat)
            {
                CommonRessourceCollectionResolve();
            }
            else if (lastInteraction == InteractionType.Type.construction)
            {
                List<AudioClip> sounds = ResourceDictionary.instance.resources["Iron"].collectingSound;
                AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
                audiosource.clip = soundFx;
                if (soundFx)
                    audiosource.Play();

                // increment progress bar
                ConstructionController construction = hoveredInteractor.GetComponent<ConstructionController>();
                if (construction && construction.Increment())
                {
                    interactionTime = 0f;
                    hoveredInteractor = null;
                }
            }
            else if (lastInteraction == InteractionType.Type.destroyBuilding)
            {
                List<AudioClip> sounds = ResourceDictionary.instance.resources["Iron"].collectingSound;
                AudioClip soundFx = sounds[Random.Range(0, sounds.Count)];
                audiosource.clip = soundFx;
                if (soundFx)
                    audiosource.Play();

                // increment progress bar
                DestructionTemplate destruction = hoveredInteractor.GetComponent<DestructionTemplate>();
                if (destruction.Increment())
                {
                    interactionTime = 0f;
                    hoveredInteractor = null;
                }
            }
        }
        else
        {
            // error
            interacting = false;
            hoveredInteractor = null;
            interactionTime = 0f;
            animator.SetBool("interaction", false);
            Debug.LogWarning("no interaction tick for this type implemented");
        }
    }



    private void ComputeInteractionConditions(InteractionType.Type type)
    {
        interactionConditionList.Clear();
        if (type == InteractionType.Type.construction || type == InteractionType.Type.forge || type == InteractionType.Type.destroyBuilding)
        {
            interactionConditionList.Add("Hammer", playerController.weapon.equipedItem.toolFamily == "Hammer" ? "ok" : "nok");
        }
        else
        {
            string[] equiped = { playerController.backpack.equipedItem.toolFamily, playerController.weapon.equipedItem.toolFamily };
            ResourceData resData = ResourceDictionary.instance.resourcesFromType[type];

            foreach (string tool in resData.tools)
            {
                string status = "nok";
                foreach (string s in equiped)
                {
                    if (s == tool)
                    {
                        if (tool == "Container")
                            status = resourceContainer.HasSpace() ? "ok" : "full";
                        else status = "ok";
                    }
                }
                interactionConditionList.Add(tool, status);
            }
        }
    }
    private bool PickableInteraction(InteractionType.Type type, GameObject interactor)
    {
        bool success = false;
        if (type == InteractionType.Type.pickableWeapon)
        {
            WeaponItem item = interactor.GetComponent<WeaponItem>();
            if (item && playerController.weapon.Equip(item.type))
            {
                if (item.forbidSecond || playerController.secondHand.equipedItem.forbidWeapon)
                    playerController.secondHand.Equip(SecondItem.Type.None);
                if (item.forbidShield)
                    playerController.shield.Equip(ShieldItem.Type.None);
                success = true;
                playerController.needEquipementAnimationUpdate = true;

                if (ToolDictionary.instance.tools.ContainsKey(playerController.weapon.equipedItem.toolFamily))
                {
                    List<AudioClip> sounds = ToolDictionary.instance.tools[playerController.weapon.equipedItem.toolFamily].collectionSound;
                    audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                    audiosource.Play();
                }
            }
        }
        else if (type == InteractionType.Type.pickableBackpack)
        {
            BackpackItem item = interactor.GetComponent<BackpackItem>();
            if (item && playerController.backpack.Equip(item.type))
                success = true;
            playerController.needEquipementAnimationUpdate = true;
            resourceContainer.Clear();

            if (ToolDictionary.instance.tools.ContainsKey(playerController.backpack.equipedItem.toolFamily))
            {
                List<AudioClip> sounds = ToolDictionary.instance.tools[playerController.backpack.equipedItem.toolFamily].collectionSound;
                audiosource.clip = sounds[Random.Range(0, sounds.Count)];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableHead)
        {
            HeadItem item = interactor.GetComponent<HeadItem>();
            if (item && playerController.head.Equip(item.type))
            {
                success = true;
                playerController.needEquipementAnimationUpdate = true;
                int index = Mathf.Clamp((int)HeadItem.getCategory(playerController.head.equipedItem.type), 0, wearHead.Count - 1);
                audiosource.clip = wearHead[index];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableSecond)
        {
            SecondItem item = interactor.GetComponent<SecondItem>();
            if (item && playerController.secondHand.Equip(item.type))
            {
                if (item.forbidWeapon || playerController.weapon.equipedItem.forbidSecond)
                    playerController.weapon.Equip(WeaponItem.Type.None);
                if (item.forbidShield)
                    playerController.shield.Equip(ShieldItem.Type.None);
                success = true;
                playerController.needEquipementAnimationUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableShield)
        {
            ShieldItem item = interactor.GetComponent<ShieldItem>();
            if (item && playerController.shield.Equip(item.type))
            {
                if (playerController.weapon.equipedItem.forbidShield)
                    playerController.weapon.Equip(WeaponItem.Type.None);
                if (playerController.secondHand.equipedItem.forbidShield)
                    playerController.secondHand.Equip(SecondItem.Type.None);
                success = true;
                playerController.needEquipementAnimationUpdate = true;
            }
        }
        else if (type == InteractionType.Type.pickableBody)
        {
            BodyItem item = interactor.GetComponent<BodyItem>();
            bool mounted = playerController.horse ? playerController.horse.equipedItem.type != HorseItem.Type.None : false;
            if (item && playerController.body.Equip(item.type, mounted))
            {
                success = true;
                playerController.needEquipementAnimationUpdate = true;
                int index = Mathf.Clamp((int)BodyItem.getCategory(playerController.body.equipedItem.type), 0, wearBody.Count - 1);
                audiosource.clip = wearBody[index];
                audiosource.Play();
            }
        }
        else if (type == InteractionType.Type.pickableHorse)
        {
            HorseItem item = interactor.GetComponent<HorseItem>();
            if ((playerController.horse && item.type == HorseItem.Type.None) || (!playerController.horse && item.type != HorseItem.Type.None))
            {
                // change player template
                PlayerController template;
                if (item.type == HorseItem.Type.None)
                    template = Arsenal.Instance.playerTemplate;
                else
                    template = Arsenal.Instance.mountedPlayerTemplate;
                PlayerController destination = Instantiate<PlayerController>(template);
                destination.gameObject.name = template.gameObject.name;

                // copy
                PlayerController.Copy(playerController, destination);
                PlayerController.MainInstance = destination;
                MapStreaming.instance.focusAgent = destination.transform;
                ConstructionSystem.instance.tpsController.target = destination.transform.Find("CameraTarget");

                destination.interactionController.PickableInteraction(type, interactor);
                Destroy(gameObject);
            }
            else
            {
                if (playerController.horse && item && playerController.horse.Equip(item.type))
                {
                    success = true;
                    playerController.needEquipementAnimationUpdate = true;
                    int index = Mathf.Clamp((int)BodyItem.getCategory(playerController.body.equipedItem.type), 0, wearBody.Count - 1);
                    audiosource.clip = wearBody[index];
                    audiosource.Play();

                    bool mounted = playerController.horse ? playerController.horse.equipedItem.type != HorseItem.Type.None : false;
                    playerController.body.Equip(playerController.body.equipedItem.type, mounted, true);
                }
            }
        }
        return success;
    }
    private bool StoreAllInteraction(InteractionType.Type type, GameObject interactor)
    {
        ResourceContainer storehouse = interactor.GetComponent<ResourceContainer>();
        if (storehouse)
        {
            Dictionary<string, int> transfers = new Dictionary<string, int>();
            Dictionary<string, int> accepted = storehouse.GetAcceptance();
            foreach (KeyValuePair<string, int> entry in resourceContainer.inventory)
            {
                if (accepted.Count == 0 || accepted.ContainsKey(entry.Key))
                {
                    int currentCount = storehouse.inventory.ContainsKey(entry.Key) ? storehouse.inventory[entry.Key] : 0;
                    int maxCount = (accepted.ContainsKey(entry.Key) && accepted[entry.Key] > 0) ? accepted[entry.Key] : storehouse.capacity;

                    if (storehouse.HasSpace() && maxCount != currentCount)
                    {
                        int maximumTransfert = Mathf.Min(entry.Value, maxCount - currentCount);
                        storehouse.AddItem(entry.Key, maximumTransfert);
                        transfers.Add(entry.Key, maximumTransfert);
                    }
                }
            }

            if (transfers.Count != 0)
            {
                foreach (KeyValuePair<string, int> entry in transfers)
                    resourceContainer.RemoveItem(entry.Key, entry.Value, false);
                resourceContainer.UpdateContent();
                playerController.RecomputeLoadFactor();
                audiosource.clip = backpackClear;
                audiosource.Play();
            }
            else
            {
                if (resourceContainer.inventory.Count == 0)
                {
                    Debug.LogWarning("Impossible to be here, pb in container load computing");
                }
                else
                {
                    interactionConditionList.Clear();
                    foreach (string acceptance in storehouse.acceptedResources)
                    {
                        string accName = acceptance;
                        if (acceptance.Contains(" "))
                        {
                            char[] separator = { ' ' };
                            accName = acceptance.Split(separator)[0];
                        }
                        interactionConditionList.Add(accName, "nor");
                    }
                    UpdateHelp();
                }
            }
        }
        return true;
    }
    private void CommonRessourceCollectionResolve()
    {
        ResourceData resData = ResourceDictionary.instance.resourcesFromType[lastInteraction];
        MapModifier.instance.grid.MakeObjectInteractable(hoveredInteractor.transform.parent.gameObject);

        // play sound, and juice, and update inventory
        AudioClip soundFx = resData.collectingSound[Random.Range(0, resData.collectingSound.Count)];
        audiosource.clip = soundFx;
        if (soundFx)
            audiosource.Play();
        int gain = Random.Range(1, 4);
        interactionJuicer.LaunchGainAnim("+" + gain.ToString(), lastInteraction);
        resourceContainer.AddItem(resData.name, gain);
        playerController.RecomputeLoadFactor();

        // decrement interactor ressource count
        CollectData data = hoveredInteractor.GetComponent<CollectData>();
        data.ressourceCount--;
        if (data.ressourceCount <= 0)
        {
            Destroy(hoveredInteractor.transform.parent.gameObject);
            interactionTime = 0;
            interacting = false;
            hoveredInteractor = null;
        }

        // stop interaction loop if needed
        if (!resourceContainer.HasSpace())
        {
            ComputeInteractionConditions(lastInteraction);
            UpdateHelp();
            interactionTime = 0f;
            interacting = false;
            animator.SetBool("interaction", false);
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(scanPosition, 2 * scanSize);
    }

    private IEnumerator HelpTimer(float t)
    {
        audiosource.Play();
        helpContainer.SetActive(true);
        yield return new WaitForSeconds(t);

        helpContainer.SetActive(false);
        foreach (Transform child in helpContainer.transform)
            Destroy(child.gameObject);
    }
}
