using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : AgentBase, IDamageable
{
    [SerializeField] int maxHealth = 1;
    [SerializeField] float health = 1;
    //[SerializeField] int team = 0;
    
    public enum BuildingStatus
    {
        Planned, Constructing, Existing, Destroying, Destroyed
    }
    public BuildingStatus status;
    /*

    [Header("Links")]
    [SerializeField] public Government government;

    [Header("Building")]
    [SerializeField] public Collider constructionBound;
    [SerializeField] public Collider collision;
    [SerializeField] bool autoBuild = false;
    [SerializeField] float constructionTime = 10;
    [SerializeField] public ActionCost cost;
    [SerializeField] int maxHealth;
   

    [Header("Production")]
    [SerializeField] bool doProduce = false;
    [SerializeField] ActionCost productionCost;
    [SerializeField] UnitAI toProduce;
    [SerializeField] float productionTime;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform waypoint;

    [Header("Render")]
    [SerializeField] new public MeshRenderer renderer;
    [SerializeField] MeshFilter filter;
    [SerializeField] Mesh[] constructionMesh;
    [SerializeField] Mesh finishedMesh;

    [Header("FX")]
    [SerializeField] ParticleSystem constructionParticles;
    [SerializeField] ParticleSystem fireParticles;
    [SerializeField] ParticleSystem explosionParticles;
    ParticleSystem[] subFireParticles;

    [Header("Status")]
    public BuildingStatus status;
    [SerializeField] float health = 1;
    [SerializeField] float construction = 0;
    [SerializeField] bool working = false;
    [SerializeField] float workerWork = 0;
    [SerializeField] float autoWork = 0;
    [SerializeField] float fire = 0;
    new protected void Start()
    {
        base.Start();
        subFireParticles = fireParticles.GetComponentsInChildren<ParticleSystem>();
        health = maxHealth;
        spawnPoint.position = World.instance.CenteredPosition(spawnPoint.position);
       // waypoint.position = spawnPoint.position;
    }
    new private void OnValidate()
    {
        base.OnValidate();
        if (!Application.isPlaying)
        {
            filter.sharedMesh = finishedMesh;
            this.gameObject.name = "Building - " + filter.sharedMesh.name;
            health = maxHealth;
        }

    }
    public void PlanForConstruction()
    {
        if (autoBuild) { construction = -1; } else { construction = -0.01f; }
        status = BuildingStatus.Planned;
        filter.sharedMesh = construction < constructionTime ? null : finishedMesh;
    }
    public void Materialize()
    {
        construction = 1;
        status = BuildingStatus.Existing;
        filter.sharedMesh = construction < constructionTime ? null : finishedMesh;
    }
    public float healthRatio { get { return health/(float)maxHealth; } }
    public float constructionRatio
    {
        get
        {
            if (construction < 1)
            {
                return construction > 0 ? construction / constructionTime : 0;
            }
            else
            {
                return autoWork / productionTime;
            }
        }
    }*/
    public void Work()
    {
        //workerWork += Time.deltaTime;
        //working = true;
    }
    /*
    public void SetWaypoint(Vector3 position)
    {
        waypoint.position = position;
    }
    public void LateUpdate()
    {
        
        if (status == BuildingStatus.Planned || status == BuildingStatus.Constructing)
        {
            selectable.isHighlighted = true;
            if (autoBuild)
            {
                workerWork = Time.deltaTime;
            }
            if (workerWork > 0)
            {
                working = true;
                construction += workerWork;
                if (construction < constructionTime)
                {
                    if (construction > 0)
                    {
                        filter.sharedMesh = constructionMesh[(int)(constructionMesh.Length * constructionRatio)];
                        constructionParticles.gameObject.SetActive(true);
                        constructionParticles.Play();
                    }
                }
                else
                {
                    construction = constructionTime;
                    filter.sharedMesh = finishedMesh;
                    status = BuildingStatus.Existing;
                    autoWork = 0;
                    constructionParticles.Stop();
                    government.FinishBuilding(this);
                }
            }
            workerWork = 0;
        }

        else if(status == BuildingStatus.Existing)
        {
            if (toProduce)
            {
                waypoint.gameObject.SetActive(selectable.isSelected);
            }
            else
            {
                waypoint.gameObject.SetActive(false);
            }
            selectable.isHighlighted = false;
            if (doProduce && toProduce && government.CanProduce(toProduce))
            {
                autoWork += Time.deltaTime;
                if (autoWork >= productionTime)
                {
                    var unit = Instantiate<UnitAI>(toProduce);
                    unit.Teleport(spawnPoint.position);
                    //if (waypoint.position != spawnPoint.position)
                    {
                        unit.GoTo(waypoint.position,false);
                    }
                    government.AddUnit(unit);
                    autoWork = 0;
                }
            }
        }

        if (healthRatio == 1)
        {
            fireParticles.gameObject.SetActive(false);
            fireParticles.Pause();
            foreach (var p in subFireParticles) p.Pause();
        }
        else if(healthRatio <= 0 && (status == BuildingStatus.Constructing || status == BuildingStatus.Existing))
        {
            explosionParticles.gameObject.SetActive(true);
            status = BuildingStatus.Destroying; 
            //fireParticles.Pause();
           // foreach (var p in subFireParticles) p.Pause();
            filter.sharedMesh = constructionMesh[0];
            health = 0;
            fire = 2;
            autoWork = 0;
            construction = 0;
        }
        else if(status!= BuildingStatus.Destroyed && healthRatio <= 0.5f)
        {
            fireParticles.gameObject.SetActive(true);
            fireParticles.Play();
            foreach (var p in subFireParticles) p.Play();
        }
        if (fire > 0)
        {
            fire -= Time.deltaTime;
            if(fire <= 0)
            {
                status = BuildingStatus.Destroyed;
                fireParticles.Stop();
                fire = 0;
            }
        }
    }

    public bool CanPerform(int actionIdx)
    {
        return toProduce && actionIdx < 2;
    }

    public bool DoPerform(int actionIdx)
    {
        if (!CanPerform(actionIdx))
        {
            return false;
        }
        else
        {
            if (actionIdx == 0)
            {
                //autoWork = 0;
                doProduce = false;
            }
            else
            {
                doProduce = true;
            }
            return true;
        }
    }
    */
    public void GetDamage(int amount)
    {
        health = Mathf.Clamp(health-amount,0, maxHealth);
    }

    public int GetTeam()
    {
        return team;
    }
}
