using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Government : MonoBehaviour
{
    [Header("General")]
    [SerializeField] [Range(0, 2)] public int team = 0;
    [SerializeField] public Color color;

    [Header("Population")]
    [SerializeField] BuildingBase main;
    [SerializeField] UnitAI peasantTemplate = null;
    [SerializeField] public int initialPopulation;
    public int MAX_POSSIBLE_POPULATION = 512;

    [Header("Resources")]
    [SerializeField] public int maxPopulation;
    [SerializeField] public float wood;
    [SerializeField] public float stone;
    [SerializeField] public float gold;
    [SerializeField] public float food;

    [Header("Visual")]
    [SerializeField] Material buildingMaterial = null;
    [SerializeField] Material unitMaterial = null;


    [Header("AI")]
    [SerializeField] float thinkInterval = 0.25f;
    [SerializeField] public List<UnitAI> population;
    [SerializeField] public List<BuildingBase> buildings;
    public float[] updateTime;
    private void Awake()
    {
        int psqrt = (int)Mathf.Sqrt(initialPopulation);
        for (int p = 0; p < initialPopulation; ++p)
        {
            {
                var peasant = Instantiate<UnitAI>(peasantTemplate);
                peasant.transform.position = peasant.transform.position + new Vector3(p / psqrt * 3, 0, p % psqrt * 3);
                peasant.transform.parent = this.transform;
                peasant.gameObject.SetActive(true);
                AddUnit(peasant);
            }
        }
        updateTime = new float[MAX_POSSIBLE_POPULATION];

        /*RegisterBuilding(main);
        FinishBuilding(main);
        main.Materialize();*/
    }
    private void Update()
    {
        for (int i = 0; i < population.Count; ++i)
        {
            UnitAI unit = population[i];
            if (unit.unit.Alive)
            {
                if (updateTime[i] == 0 || updateTime[i] > thinkInterval)
                {
                    unit.Sense();
                    updateTime[i] = 0;
                }
                unit.Think();
                unit.Act();
                updateTime[i] += Time.deltaTime * Random.Range(0.9f, 1.1f);
            }
        }
    }
    public void Collect(ResourceSource.ResourceType resource)
    {
        switch (resource)
        {
            case ResourceSource.ResourceType.Food:
                ++food;
                break;
            case ResourceSource.ResourceType.Stone:
                ++stone;
                break;
            case ResourceSource.ResourceType.Gold:
                ++gold;
                break;
            case ResourceSource.ResourceType.Wood:
                ++wood;
                break;
        }
    }

    public bool CanBuild(BuildingBase building)
    {
       /* ActionCost cost = building.cost;
        return cost.food < this.food
            && cost.wood < this.wood
            && cost.gold < this.gold
            && cost.stone < this.stone;*/
            return true;
    }

    public void RegisterBuilding(BuildingBase building)
    {
        buildings.Add(building);
        //building.government = this;
        building.team = this.team;
        building.transform.parent = this.transform;
        building.tag = this.tag;
        foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.sharedMaterial = buildingMaterial;
        }
    }
    public void PayBuilding(BuildingBase building)
    {
        /*ActionCost cost = building.cost;
        this.food -= cost.food;
        this.wood -= cost.wood;
        this.gold -= cost.gold;
        this.stone -= cost.stone;*/

        RegisterBuilding(building);
    }
    public void FinishBuilding(BuildingBase building)
    {
        //this.maxPopulation += building.cost.population;
    }
    public void RemoveBuilding(BuildingBase building)
    {
        //this.maxPopulation -= building.cost.population;
        buildings.Remove(building);
    }
    public bool CanProduce(UnitAI unit)
    {
        return population.Count < maxPopulation;
    }
    public void AddUnit(UnitAI unit)
    {
        if (population.Count < MAX_POSSIBLE_POPULATION)
        {
            population.Add(unit);
            unit.unit.government = this;
            unit.unit.team = this.team;

            unit.transform.parent = this.transform;
            unit.tag = this.tag;
            //TODO: Optimize this
            foreach (var renderer in unit.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.sharedMaterial = unitMaterial;
            }
            foreach (var renderer in unit.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.sharedMaterial = unitMaterial;
            }
        }
        else
        {
            Debug.LogError("TOO MANY PEOPLE");
        }
    }
    public void RemoveUnit(UnitAI unit)
    {
        population.Remove(unit);
    }
}
