using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arsenal : MonoBehaviour
{
    [Header("Shop related")]
    public bool shopOnStart = false;
    public bool createIcons = false;
    public SpecialPickableShopArsenal pickablePrefab;
    public Transform cameraPivot;
    public new Camera camera;
    public string iconFolderPath = "";
    public Vector2Int iconSize;

    [Header("Player templates")]
    public PlayerController playerTemplate;
    public PlayerController mountedPlayerTemplate;

    [Header("Backpack items")]
    public List<BackpackItem> backpackObjectList;
    public Dictionary<BackpackItem.Type, BackpackItem> backpackDictionary;

    [Header("Weapons items")]
    public List<WeaponItem> weaponObjectList;
    public Dictionary<WeaponItem.Type, WeaponItem> weaponDictionary;

    [Header("Heads items")]
    public List<HeadItem> headObjectList;
    public Dictionary<HeadItem.Type, HeadItem> headDictionary;

    [Header("Second hand items")]
    public List<SecondItem> secondObjectList;
    public Dictionary<SecondItem.Type, SecondItem> secondDictionary;

    [Header("Shield items")]
    public List<ShieldItem> shieldObjectList;
    public Dictionary<ShieldItem.Type, ShieldItem> shieldDictionary;

    [Header("Bodies items")]
    public List<BodyItem> bodyObjectList;
    public Dictionary<BodyItem.Type, BodyItem> bodyDictionary;
    public List<BodyItem> mountedBodyObjectList;
    public Dictionary<BodyItem.Type, BodyItem> mountedBodyDictionary;

    [Header("Horses items")]
    public List<HorseItem> horseObjectList;
    public Dictionary<HorseItem.Type, HorseItem> horseDictionary;

    [Header("Animations clips")]
    public AnimationClip[] archeryConfiguration = new AnimationClip[4];
    public AnimationClip[] crossbowConfiguration = new AnimationClip[4];
    public AnimationClip[] staffConfiguration = new AnimationClip[4];
    public AnimationClip[] spearConfiguration = new AnimationClip[4];
    public AnimationClip[] twoHandedConfiguration = new AnimationClip[4];
    public AnimationClip[] polearmConfiguration = new AnimationClip[4];
    public AnimationClip[] shieldedConfiguration = new AnimationClip[4];
    public AnimationClip[] defaultConfiguration = new AnimationClip[4];

    [Header("Mounted animations clips")]
    public AnimationClip[] mountedArcheryConfiguration;
    public AnimationClip[] mountedCrossbowConfiguration;
    public AnimationClip[] mountedSpearConfiguration;
    public AnimationClip[] mountedSpearOffensiveConfiguration;
    public AnimationClip[] mountedDefaultConfiguration;

    // Singleton struct
    private static Arsenal _instance = null;
    public static Arsenal Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            InitializeTables();
        }
    }

    // Initialization
    void Start()
    {
        // special debug feature
        if (shopOnStart || createIcons)
            InstanciateShop();
    }

    // Get item from type
    public BackpackItem Get(BackpackItem.Type type, bool verbose = true)
    {
        if (backpackDictionary.ContainsKey(type))
            return backpackDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : backpack dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public WeaponItem Get(WeaponItem.Type type, bool verbose = true)
    {
        if (weaponDictionary.ContainsKey(type))
            return weaponDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : weapon dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public HeadItem Get(HeadItem.Type type, bool verbose = true)
    {
        if (headDictionary.ContainsKey(type))
            return headDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : head dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public SecondItem Get(SecondItem.Type type, bool verbose = true)
    {
        if (secondDictionary.ContainsKey(type))
            return secondDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : second hand dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public ShieldItem Get(ShieldItem.Type type, bool verbose = true)
    {
        if (shieldDictionary.ContainsKey(type))
            return shieldDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : shield dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public BodyItem Get(BodyItem.Type type, bool mounted, bool verbose = true)
    {
        if(mounted)
        {
            if (mountedBodyDictionary.ContainsKey(type))
                return mountedBodyDictionary[type];
        }
        else
        {
            if (bodyDictionary.ContainsKey(type))
                return bodyDictionary[type];
        }
        if (verbose)
            Debug.LogError("Arsenal : body dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public HorseItem Get(HorseItem.Type type, bool verbose = true)
    {
        if (horseDictionary.ContainsKey(type))
            return horseDictionary[type];
        if (verbose)
            Debug.LogError("Arsenal : body dictionary doesn't contain item " + type.ToString());
        return null;
    }

    // Get pickable item 
    public GameObject GetPickable(BackpackItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        BackpackItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableBackpack;
        BackpackItem.Copy(item, go.AddComponent<BackpackItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(WeaponItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        WeaponItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableWeapon;
        WeaponItem.Copy(item, go.AddComponent<WeaponItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(HeadItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        HeadItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableHead;
        HeadItem.Copy(item, go.AddComponent<HeadItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(SecondItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        SecondItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableSecond;
        SecondItem.Copy(item, go.AddComponent<SecondItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(ShieldItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        ShieldItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableShield;
        ShieldItem.Copy(item, go.AddComponent<ShieldItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(BodyItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        BodyItem weaponItem = Get(type, false);
        if (!weaponItem)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableBody;
        BodyItem.Copy(weaponItem, go.AddComponent<BodyItem>());
        go.SetActive(true);

        MeshFilter mf = weaponItem.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }
    public GameObject GetPickable(HorseItem.Type type, bool showName = false, bool destroyOnPick = true)
    {
        HorseItem item = Get(type);
        if (!item)
            return null;

        GameObject go = Instantiate(pickablePrefab.gameObject);
        go.name = type.ToString();
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.AddComponent<InteractionType>().type = InteractionType.Type.pickableHorse;
        HorseItem.Copy(item, go.AddComponent<HorseItem>());
        go.SetActive(true);

        MeshFilter mf = item.gameObject.GetComponent<MeshFilter>();
        SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
        pickable.textmesh.text = go.name;
        if (go.name.Length >= 8)
            pickable.textmesh.characterSize *= 0.5f;
        if (mf) pickable.itemMesh.mesh = mf.mesh;
        else pickable.itemMesh.gameObject.SetActive(false);
        pickable.body.gameObject.SetActive(false);
        pickable.textmesh.gameObject.SetActive(showName);
        go.GetComponent<Item>().destroyOnPick = destroyOnPick;

        return go;
    }

    // Get animation configuration regarding of the equipement
    public AnimationClip[] GetAnimationClip(ref WeaponItem weapon, ref SecondItem second, ref ShieldItem shield, ref BodyItem body, ref HeadItem head, ref BackpackItem backpack)
    {
        if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 5) return spearConfiguration;
        else if (second.type != SecondItem.Type.None && second.animationCode == 2) return archeryConfiguration;
        else if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 3) return crossbowConfiguration;
        else if (shield.type != ShieldItem.Type.None) return shieldedConfiguration;
        else if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 4) return twoHandedConfiguration;
        else if (weapon.type != WeaponItem.Type.None && second.animationCode == 6) return staffConfiguration;
        else if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 7) return polearmConfiguration;
        else return defaultConfiguration;
    }
    public AnimationClip[] GetMountedAnimationClip(ref WeaponItem weapon, ref SecondItem second, ref ShieldItem shield, ref BodyItem body, ref HeadItem head, ref BackpackItem backpack)
    {
        if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 5) return mountedSpearConfiguration;
        else if (second.type != SecondItem.Type.None && second.animationCode == 2) return mountedArcheryConfiguration;
        else if (weapon.type != WeaponItem.Type.None && weapon.animationCode == 3) return mountedCrossbowConfiguration;
        else return mountedDefaultConfiguration;
    }

    // Initialization
    public void InitializeTables()
    {
        // create backpack association table
        backpackDictionary = new Dictionary<BackpackItem.Type, BackpackItem>();
        foreach (BackpackItem b in backpackObjectList)
        {
            if (!backpackDictionary.ContainsKey(b.type))
            {
                backpackDictionary[b.type] = b;
                b.gameObject.name = b.type.ToString();
                if (!b.SearchIcon("Icons/Backpacks/" + b.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + b.type.ToString());
                if(b.itemName.Length == 0)
                    b.itemName = GetFormatedName(b.type.ToString());
            }
            else Debug.LogError("In Arsenal : The backpack dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }


        // create weapons association table
        weaponDictionary = new Dictionary<WeaponItem.Type, WeaponItem>();
        foreach (WeaponItem w in weaponObjectList)
        {
            if (!weaponDictionary.ContainsKey(w.type))
            {
                weaponDictionary[w.type] = w;
                w.gameObject.name = w.type.ToString();
                if (!w.SearchIcon("Icons/Weapons/" + w.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + w.type.ToString());
                if (w.itemName.Length == 0)
                    w.itemName = GetFormatedName(w.type.ToString());
            }
            else Debug.LogError("In Arsenal : The weapon dictionary already contain an item of this type, see gameObject " + w.gameObject);
        }

        // create weapons association table
        headDictionary = new Dictionary<HeadItem.Type, HeadItem>();
        foreach (HeadItem h in headObjectList)
        {
            if (!headDictionary.ContainsKey(h.type))
            {
                headDictionary[h.type] = h;
                h.gameObject.name = h.type.ToString();
                if (!h.SearchIcon("Icons/Heads/" + h.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + h.type.ToString());
                if (h.itemName.Length == 0)
                    h.itemName = GetFormatedName(h.type.ToString());
            }
            else Debug.LogError("In Arsenal : The head dictionary already contain an item of this type, see gameObject " + h.gameObject);
        }

        // create weapons association table
        secondDictionary = new Dictionary<SecondItem.Type, SecondItem>();
        foreach (SecondItem s in secondObjectList)
        {
            if (!secondDictionary.ContainsKey(s.type))
            {
                secondDictionary[s.type] = s;
                s.gameObject.name = s.type.ToString();
                if (!s.SearchIcon("Icons/SecondHands/" + s.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + s.type.ToString());
                if (s.itemName.Length == 0)
                    s.itemName = GetFormatedName(s.type.ToString());
            }
            else Debug.LogError("In Arsenal : The second hand dictionary already contain an item of this type, see gameObject " + s.gameObject);
        }

        // create weapons association table
        shieldDictionary = new Dictionary<ShieldItem.Type, ShieldItem>();
        foreach (ShieldItem s in shieldObjectList)
        {
            if (!shieldDictionary.ContainsKey(s.type))
            {
                shieldDictionary[s.type] = s;
                s.gameObject.name = s.type.ToString();
                if (!s.SearchIcon("Icons/Shields/" + s.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + s.type.ToString());
                if (s.itemName.Length == 0)
                    s.itemName = GetFormatedName(s.type.ToString());
            }
            else Debug.LogError("In Arsenal : The shield dictionary already contain an item of this type, see gameObject " + s.gameObject);
        }

        // create weapons association table
        bodyDictionary = new Dictionary<BodyItem.Type, BodyItem>();
        foreach (BodyItem b in bodyObjectList)
        {
            if (!bodyDictionary.ContainsKey(b.type))
            {
                bodyDictionary[b.type] = b;
                b.gameObject.name = b.type.ToString();
                if (!b.SearchIcon("Icons/Bodies/" + b.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + b.type.ToString());
                if (b.itemName.Length == 0)
                    b.itemName = GetFormatedName(b.type.ToString());
            }
            else Debug.LogError("In Arsenal : The body dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }
        mountedBodyDictionary = new Dictionary<BodyItem.Type, BodyItem>();
        foreach (BodyItem b in mountedBodyObjectList)
        {
            if (!mountedBodyDictionary.ContainsKey(b.type))
            {
                mountedBodyDictionary[b.type] = b;
                b.gameObject.name = b.type.ToString();
                if (!b.SearchIcon("Icons/Bodies/" + b.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + b.type.ToString());
                if (b.itemName.Length == 0)
                    b.itemName = GetFormatedName(b.type.ToString());
            }
            else Debug.LogError("In Arsenal : The mounted body dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }

        // create horses association table
        horseDictionary = new Dictionary<HorseItem.Type, HorseItem>();
        foreach (HorseItem h in horseObjectList)
        {
            if (!horseDictionary.ContainsKey(h.type))
            {
                horseDictionary[h.type] = h;
                h.gameObject.name = h.type.ToString();
                if (!h.SearchIcon("Icons/Horses/" + h.type.ToString()))
                    Debug.LogWarning("Cannot found icon for item " + h.type.ToString());
                if (h.itemName.Length == 0)
                    h.itemName = GetFormatedName(h.type.ToString());
            }
            else Debug.LogError("In Arsenal : The horse dictionary already contain an item of this type, see gameObject " + h.gameObject);
        }
    }

    // Instanciate the shop
    public float gapY = 5f;
    private void InstanciateShop()
    {
        GameObject shopContainer = new GameObject("shopContainer");
        shopContainer.transform.parent = transform;
        shopContainer.transform.localPosition = Vector3.zero;
        shopContainer.transform.localRotation = Quaternion.identity;
        shopContainer.transform.localScale = Vector3.one;
        shopContainer.SetActive(true);

        float gap = 3f;

        // backpack
        Vector3 position = Vector3.zero;
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = BackpackItem.Type.None.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(-2*gap, position.y, position.z);
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableBackpack;
            go.AddComponent<BackpackItem>();
            go.SetActive(true);
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.body.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<BackpackItem.Type, BackpackItem> item in backpackDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableBackpack;
            BackpackItem.Copy(item.Value, go.AddComponent<BackpackItem>());
            go.SetActive(true);

            MeshFilter mf = item.Value.gameObject.GetComponent<MeshFilter>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            if (mf) pickable.itemMesh.mesh = mf.mesh;
            else pickable.itemMesh.gameObject.SetActive(false);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Backpacks/" + go.name + ".png");
            }
            position.x += gap;
        }

        // shields
        position.x = 0; position.z -= gap; position.y += gapY;
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = ShieldItem.Type.None.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(-2 * gap, position.y, position.z);
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableShield;
            go.AddComponent<ShieldItem>();
            go.SetActive(true);
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.body.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<ShieldItem.Type, ShieldItem> item in shieldDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableShield;
            ShieldItem.Copy(item.Value, go.AddComponent<ShieldItem>());
            go.SetActive(true);

            MeshFilter mf = item.Value.gameObject.GetComponent<MeshFilter>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            if (mf) pickable.itemMesh.mesh = mf.mesh;
            else pickable.itemMesh.gameObject.SetActive(false);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Shields/" + go.name + ".png");
            }
            position.x += gap;
        }

        // second hand
        position.x = 0; position.z -= gap; position.y += gapY;
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = SecondItem.Type.None.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(-2 * gap, position.y, position.z);
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableSecond;
            go.AddComponent<SecondItem>();
            go.SetActive(true);
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.body.gameObject.SetActive(false);

        }
        foreach (KeyValuePair<SecondItem.Type, SecondItem> item in secondDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableSecond;
            SecondItem.Copy(item.Value, go.AddComponent<SecondItem>());
            go.SetActive(true);

            MeshFilter mf = item.Value.gameObject.GetComponent<MeshFilter>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            if (mf) pickable.itemMesh.mesh = mf.mesh;
            else pickable.itemMesh.gameObject.SetActive(false);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/SecondHands/" + go.name + ".png");
            }
            position.x += gap;
        }

        // weapons
        position.x = 0; position.z -= gap; position.y += gapY;
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = WeaponItem.Type.None.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(-2 * gap, position.y, position.z);
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableWeapon;
            go.AddComponent<WeaponItem>();
            go.SetActive(true);
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.body.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<WeaponItem.Type, WeaponItem> item in weaponDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableWeapon;
            WeaponItem.Copy(item.Value, go.AddComponent<WeaponItem>());
            go.SetActive(true);

            MeshFilter mf = item.Value.gameObject.GetComponent<MeshFilter>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            if (mf) pickable.itemMesh.mesh = mf.mesh;
            else pickable.itemMesh.gameObject.SetActive(false);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Weapons/" + go.name + ".png");
            }
            position.x += gap;
        }

        // heads
        position.x = 0; position.z -= gap; position.y += gapY;
        foreach (KeyValuePair<HeadItem.Type, HeadItem> item in headDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableHead;
            HeadItem.Copy(item.Value, go.AddComponent<HeadItem>());
            go.SetActive(true);

            MeshFilter mf = item.Value.gameObject.GetComponent<MeshFilter>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            if (mf) pickable.itemMesh.mesh = mf.mesh;
            else pickable.itemMesh.gameObject.SetActive(false);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Heads/" + go.name + ".png");
            }
            position.x += gap;
        }

        // bodies
        position.x = 0; position.z -= gap; position.y += gapY;
        foreach (KeyValuePair<BodyItem.Type, BodyItem> item in bodyDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableBody;
            BodyItem.Copy(item.Value, go.AddComponent<BodyItem>());
            go.SetActive(true);

            SkinnedMeshRenderer skin = item.Value.gameObject.GetComponent<SkinnedMeshRenderer>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.itemMesh.gameObject.SetActive(false);

            if (skin) BodySlot.CopySkinnedMesh(skin, pickable.body);
            else pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Bodies/" + go.name + ".png");
            }
            position.x += gap;
        }

        // horses
        position.x = 0; position.z -= 2 * gap; position.y += gapY;
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = HorseItem.Type.None.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(-2 * gap, position.y, position.z);
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableHorse;
            go.AddComponent<HorseItem>();
            go.SetActive(true);
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.body.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<HorseItem.Type, HorseItem> item in horseDictionary)
        {
            GameObject go = Instantiate(pickablePrefab.gameObject);
            go.name = item.Key.ToString();
            go.transform.parent = shopContainer.transform;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = position;
            go.AddComponent<InteractionType>().type = InteractionType.Type.pickableHorse;
            HorseItem.Copy(item.Value, go.AddComponent<HorseItem>());
            go.SetActive(true);

            SkinnedMeshRenderer skin = item.Value.gameObject.GetComponent<SkinnedMeshRenderer>();
            SpecialPickableShopArsenal pickable = go.GetComponent<SpecialPickableShopArsenal>();
            pickable.textmesh.text = go.name;
            if (go.name.Length >= 8)
                pickable.textmesh.characterSize *= 0.5f;
            pickable.itemMesh.gameObject.SetActive(false);

            if (skin) BodySlot.CopySkinnedMesh(skin, pickable.horse);
            else pickable.horse.gameObject.SetActive(false);
            pickable.horse.gameObject.SetActive(true);
            pickable.body.gameObject.SetActive(false);

            if (createIcons)
            {
                pickable.textmesh.gameObject.SetActive(false);
                cameraPivot.position = go.transform.position + new Vector3(0, 1, 3f);
                CreateIcon(iconFolderPath + "/Horses/" + go.name + ".png");
            }
            position.x += gap;
        }
    }

    private void CreateIcon(string fileName)
    {
        if(createIcons)
        {
            RenderTexture rt = new RenderTexture(iconSize.x, iconSize.y, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(iconSize.x, iconSize.y, TextureFormat.RGBA32, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, iconSize.x, iconSize.y), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(fileName, bytes);
        }
    }
    private string GetFormatedName(string inputName)
    {
        string result = "";
        for (int i = 0; i < inputName.Length; i++)
        {
            char c = inputName[i];
            if (i == 0)
                result += c;
            else if (c >= 'a' && c <= 'z')
                result += c;
            else if (c >= 'A' && c <= 'Z' && i != inputName.Length - 1)
                result += " " + (char)(c + 32); // add space and switch to lower case
        }
        return result;
    }
}
