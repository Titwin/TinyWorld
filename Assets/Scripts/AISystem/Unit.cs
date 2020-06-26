using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : AgentBase, IDamageable
{
    [Header("Links")]
    public Government government;
    public UnitAI controller;
    public enum UnitState
    {
        Idle, Walking, Running, Attacking_windup, Attacking_relase, Attacking_windown, Working, Interacting, Waiting, Falling, Dead, Free
    }
    [Header("Attributes")]
    [SerializeField] public int MAX_HP;
    [SerializeField] public float MAX_SPEED = 5;
    [SerializeField] public float MAX_ACCELERATION = 5;
    [SerializeField] public float SPEED_ROTATION = 1;

    [Header("Equipment")]
    [SerializeField] public List<EquipData> inventory;
    [SerializeField] public WeaponData weapon;
    [SerializeField] public WeaponItem.Type tool;
    [SerializeField] public WeaponItem.Type mainWeapon;
    [SerializeField] public ShieldItem.Type shield;
    [SerializeField] public SecondItem.Type SecondaryWeapon;
    [SerializeField] public HeadItem.Type head;
    [SerializeField] public BodyItem.Type body;
    [SerializeField] public BackpackItem.Type backpack;
    [SerializeField] public List<ResourceSource.ResourceType> collected = new List<ResourceSource.ResourceType>();

    [Header("Status")]
    [SerializeField] private UnitState previousState = UnitState.Idle;
    [SerializeField] private UnitState state = UnitState.Idle;
    [SerializeField] public int hp;
    
    public delegate void OnChange();
    public OnChange StateChanged;
    public OnChange OnDamageReceived;
    public OnChange OnEquipmentChanged;

    protected /*override*/ void OnValidate()
    {
        //base.OnValidate();
        controller = GetComponent<UnitAI>();
    }
    protected/* override*/ void OnCreate()
    {
        //base.OnCreate();
        if (weapon)
        {
            mainWeapon = weapon.type;
            SecondaryWeapon = weapon.type2;
        }
        hp = MAX_HP;
    }

    public UnitState State { 
        get => state; 
        set {
            if (value != state)
            {
                previousState = state;
                state = value;
                StateChanged?.Invoke();
            }
        }
    }

    public UnitState PreviousState
    {
        get => previousState;
    }

    public float HealthRatio { get { return hp / (float)MAX_HP; } }
    public bool Alive { get { return state != UnitState.Dead && state != UnitState.Free; } }
    public void GetDamage(int amount)
    {
        if (hp > 0)
        {
            hp -= amount;
            if (hp <= 0)
            {
                State = UnitState.Dead;
            }
            else
            {
                OnDamageReceived?.Invoke();
            }
        }
    }

    public void DoWorkOn(BuildingBase building)
    {
        building.Work();
    }
    public void DoWorkOn(ResourceSource resource)
    {
        if (resource.Extract())
        {
            collected.Add(resource.resource);
            if (collected.Count >= 10)
            {
                DoDepositOn(null);
            }
            //government.Collect(resource.resource);
        }
    }
    public void DoDepositOn(BuildingBase building)
    {
        foreach(var resource in collected)
        {
            government.Collect(resource);
        }
        collected.Clear();
    }

    public void DoAttack(IDamageable damageable)
    {
        if (weapon != null)
        {
            damageable.GetDamage(weapon.damage);
        }
        else
        {
            Debug.LogWarning(this.name + " has no weapon!");
        }
    }

    public int GetTeam()
    {
        return team;
    }
}
