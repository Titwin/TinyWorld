﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabsContainer : MonoBehaviour
{
    public int diversity = 10;

    public Dirt originalDirt;
    private Mesh[] dirtMeshes;
    public Water originalWater;
    private Water[] waterPool;

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
        waterPool = new Water[5 * diversity + 1];
        InitDirt();
        InitWater();
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
    
    private void InitWater()
    {
        GameObject container = new GameObject();
        container.name = originalWater.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < diversity; i++)
            {
                GameObject go = Instantiate(originalWater.gameObject);
                go.transform.parent = container.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Water water = go.GetComponent<Water>();

                switch (j)
                {
                    case 0: water.Initialize(false, false, false, false, 0.3f); break;
                    case 1: water.Initialize(false, true, false, false, 0.3f); break;
                    case 2: water.Initialize(false, false, true, true, 0.3f); break;
                    case 3: water.Initialize(false, true, true, false, 0.3f); break;
                    case 4: water.Initialize(false, true, true, true, 0.3f); break;
                    case 5: water.Initialize(true, true, true, true, 0.3f); break;
                    default: break;
                }

                waterPool[j * diversity + i] = water;

                if (j == 5) break; // only one of type F
            }
        }

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

    public int GetSeed() { return Random.Range(0, diversity); }

    public Mesh GetDirtA( ) { return dirtMeshes[5 * diversity]; }
    public Mesh GetDirtB( ) { return dirtMeshes[4 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtC( ) { return dirtMeshes[2 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtD( ) { return dirtMeshes[3 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtE( ) { return dirtMeshes[1 * diversity + Random.Range(0, diversity)]; }
    public Mesh GetDirtF( ) { return dirtMeshes[Random.Range(0, diversity)]; }

    public Mesh GetWaterA(int seed) { return waterPool[5 * diversity].ground.sharedMesh; }
    public Mesh GetWaterB(int seed) { return waterPool[4 * diversity + seed].ground.sharedMesh; }
    public Mesh GetWaterC(int seed) { return waterPool[2 * diversity + seed].ground.sharedMesh; }
    public Mesh GetWaterD(int seed) { return waterPool[3 * diversity + seed].ground.sharedMesh; }
    public Mesh GetWaterE(int seed) { return waterPool[1 * diversity + seed].ground.sharedMesh; }
    public Mesh GetWaterF(int seed) { return waterPool[Mathf.Min(diversity - 1, seed)].ground.sharedMesh; }

    public Mesh GetWaterColliderA(int seed) { return waterPool[5 * diversity].waterCollider.sharedMesh; }
    public Mesh GetWaterColliderB(int seed) { return waterPool[4 * diversity + seed].waterCollider.sharedMesh; }
    public Mesh GetWaterColliderC(int seed) { return waterPool[2 * diversity + seed].waterCollider.sharedMesh; }
    public Mesh GetWaterColliderD(int seed) { return waterPool[3 * diversity + seed].waterCollider.sharedMesh; }
    public Mesh GetWaterColliderE(int seed) { return waterPool[1 * diversity + seed].waterCollider.sharedMesh; }
    public Mesh GetWaterColliderF(int seed) { return waterPool[seed].waterCollider.sharedMesh; }
    
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
