using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabsContainer : MonoBehaviour
{
    public int diversity = 10;
    public Grass originalGrass;
    private Mesh[] grassMeshes;

    public Dirt originalDirt;
    private Mesh[] dirtMeshes;

    public GameObject originalWheet;
    private Mesh wheetMeshes;

    public List<GameObject> bigStones;
    public List<GameObject> midStones;
    public List<GameObject> smallStones;

    public List<OreComponent> ores = new List<OreComponent>();
    public List<Material> oreMaterialsList = new List<Material>();
    private Dictionary<string, Material> oreMaterials;

    public List<Material> ressourceMaterialList;

    // Singleton struct
    private static TilePrefabsContainer _instance;
    public static TilePrefabsContainer Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        dirtMeshes = new Mesh[5 * diversity + 1];
        grassMeshes = new Mesh[9 * diversity];
        InitDirt();
        InitGrass();
        InitWheet();
        InitStone();
    }

    private void InitDirt()
    {
        GameObject container = new GameObject();
        container.name = originalDirt.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < diversity; i++)
            {
                GameObject go = Instantiate(originalDirt.gameObject);
                go.transform.parent = container.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Dirt dirt = go.GetComponent<Dirt>();

                switch (j)
                {
                    case 0: dirt.Initialize(false, false, false, false, 0.3f); break;
                    case 1: dirt.Initialize(false, true, false, false, 0.3f); break;
                    case 2: dirt.Initialize(false, false, true, true, 0.3f); break;
                    case 3: dirt.Initialize(false, true, true, false, 0.3f); break;
                    case 4: dirt.Initialize(false, true, true, true, 0.3f); break;
                    case 5: dirt.Initialize(true, true, true, true, 0.3f); break;
                    default: break;
                }

                dirtMeshes[j * diversity + i] = dirt.meshFilter.sharedMesh;
                if (j == 5)
                    break;
            }
        }

        container.SetActive(false);
    }

    private void InitGrass()
    {
        GameObject container = new GameObject();
        container.name = originalGrass.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        for (int j = 0; j < 9; j++) 
        {
            for (int i = 0; i < diversity; i++)
            {
                GameObject go = Instantiate(originalGrass.gameObject);
                go.transform.parent = container.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Grass grass = go.GetComponent<Grass>();
                grass.Initialize(j);
                grassMeshes[j * diversity + i] = grass.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        container.SetActive(false);
    }

    private void InitWheet()
    {
        GameObject container = new GameObject();
        container.name = originalWheet.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        GameObject go = Instantiate(originalWheet.gameObject);
        go.transform.parent = container.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

        Wheat wheet = go.GetComponentInChildren<Wheat>();
        wheet.Initialize();
        wheetMeshes = wheet.GetComponent<MeshFilter>().sharedMesh;

        container.SetActive(false);
    }

    private void InitStone()
    {
        GameObject container = new GameObject();
        container.name = "Ore_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        List<GameObject> stoneList = new List<GameObject>();
        stoneList.AddRange(bigStones);
        stoneList.AddRange(midStones);
        stoneList.AddRange(smallStones);

        foreach(GameObject go in stoneList)
        {
            OreComponent ore = go.GetComponent<OreComponent>();
            if(ore)
            {
                GameObject newore = Instantiate(go);
                newore.name += "_ore";                    
                newore.transform.parent = container.transform;
                newore.transform.localPosition = go.transform.localPosition + new Vector3(0, 6f, 0);

                OreComponent ore2 = newore.GetComponent<OreComponent>();
                ores.Add(ore2);

                foreach (GameObject orePart in ore.orePlacement)
                    DestroyImmediate(orePart);
                DestroyImmediate(ore);
            }
        }
        container.SetActive(false);

        oreMaterials = new Dictionary<string, Material>();
        foreach(Material m in oreMaterialsList)
        {
            oreMaterials[m.name] = m;
        }
    }

    public Mesh GetDirtA() { return dirtMeshes[5 * diversity]; }
    public Mesh GetDirtB() { return dirtMeshes[4 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtC() { return dirtMeshes[2 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtD() { return dirtMeshes[3 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtE() { return dirtMeshes[1 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtF() { return dirtMeshes[Random.Range(0, diversity - 1)]; }

    public Mesh GetGrass(int n) { return grassMeshes[n * diversity + Random.Range(0, diversity - 1)]; }

    public Mesh GetWheet() { return wheetMeshes; }

    public GameObject GetStone(int size)
    {
        size = Mathf.Clamp(size, 0, 2);
        switch(size)
        {
            case 0: return smallStones[Random.Range(0, smallStones.Count - 1)];
            case 1: return midStones[Random.Range(0, midStones.Count - 1)];
            case 2: return bigStones[Random.Range(0, bigStones.Count - 1)];
            default: return null;
        }
    }
    public GameObject GetStone(int size, int index)
    {
        size = Mathf.Clamp(size, 0, 2);
        switch (size)
        {
            case 0:
                index = Mathf.Clamp(index, 0, smallStones.Count - 1);
                return smallStones[index];
            case 1:
                index = Mathf.Clamp(index, 0, midStones.Count - 1);
                return midStones[index];
            case 2:
                index = Mathf.Clamp(index, 0, bigStones.Count - 1);
                return bigStones[index];
            default: return null;
        }
    }
    public GameObject GetOre(string ressource)
    {
        GameObject go = Instantiate(ores[Random.Range(0, ores.Count - 1)].gameObject);
        GameObject interactor = go.transform.Find("Interactor").gameObject;
        InteractionType interaction = interactor.GetComponent<InteractionType>();

        Material m = oreMaterials[ressource];
        foreach (GameObject ore in go.GetComponent<OreComponent>().orePlacement)
            ore.GetComponent<MeshRenderer>().material = m;

        switch(ressource)
        {
            case "Iron": interaction.type = InteractionType.Type.collectIron; break;
            case "Gold": interaction.type = InteractionType.Type.collectGold; break;
            case "Crystal": interaction.type = InteractionType.Type.collectCrystal; break;
            default: break;
        }

        return go;
    }
}
