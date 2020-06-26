using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arsenal : MonoBehaviour
{
    [Header("Shop related")]
    public bool shopOnStart = false;
    public SpecialPickableShopArsenal pickablePrefab;

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
        if (shopOnStart)
            InstanciateShop();
    }

    // Get item from type
    public BackpackItem Get(BackpackItem.Type type)
    {
        if (backpackDictionary.ContainsKey(type))
            return backpackDictionary[type];
        Debug.LogError("Arsenal : backpack dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public WeaponItem Get(WeaponItem.Type type)
    {
        if (weaponDictionary.ContainsKey(type))
            return weaponDictionary[type];
        Debug.LogError("Arsenal : weapon dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public HeadItem Get(HeadItem.Type type)
    {
        if (headDictionary.ContainsKey(type))
            return headDictionary[type];
        Debug.LogError("Arsenal : head dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public SecondItem Get(SecondItem.Type type)
    {
        if (secondDictionary.ContainsKey(type))
            return secondDictionary[type];
        Debug.LogError("Arsenal : second hand dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public ShieldItem Get(ShieldItem.Type type)
    {
        if (shieldDictionary.ContainsKey(type))
            return shieldDictionary[type];
        Debug.LogError("Arsenal : shield dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public BodyItem Get(BodyItem.Type type, bool mounted)
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
        Debug.LogError("Arsenal : body dictionary doesn't contain item " + type.ToString());
        return null;
    }
    public HorseItem Get(HorseItem.Type type)
    {
        if (horseDictionary.ContainsKey(type))
            return horseDictionary[type];
        Debug.LogError("Arsenal : body dictionary doesn't contain item " + type.ToString());
        return null;
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
            }
            else Debug.LogError("In Arsenal : The body dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }
        mountedBodyDictionary = new Dictionary<BodyItem.Type, BodyItem>();
        foreach (BodyItem b in mountedBodyObjectList)
        {
            if (!mountedBodyDictionary.ContainsKey(b.type))
            {
                mountedBodyDictionary[b.type] = b;
            }
            else Debug.LogError("In Arsenal : The body dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }

        // create horses association table
        horseDictionary = new Dictionary<HorseItem.Type, HorseItem>();
        foreach (HorseItem s in horseObjectList)
        {
            if (!horseDictionary.ContainsKey(s.type))
            {
                horseDictionary[s.type] = s;
            }
            else Debug.LogError("In Arsenal : The horse dictionary already contain an item of this type, see gameObject " + s.gameObject);
        }
    }

    // Instanciate the shop
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

            position.x += gap;
        }

        // shields
        position.x = 0; position.z -= gap;
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

            position.x += gap;
        }

        // second hand
        position.x = 0; position.z -= gap;
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

            position.x += gap;
        }

        // weapons
        position.x = 0; position.z -= gap;
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

            position.x += gap;
        }

        // heads
        position.x = 0; position.z -= gap;
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

            position.x += gap;
        }

        // bodies
        position.x = 0; position.z -= gap;
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

            position.x += gap;
        }

        // horses
        position.x = 0; position.z -= 2 * gap;
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

            position.x += gap;
        }
    }
}
