using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Custom/Weapon Data", order = 1)]
public class WeaponData : EquipData
{
    [Header("Visual")]
    [SerializeField] public Mesh mesh;
    public int animationCode = 1;

    public enum DamageType
    {
        Cutting, Piercing, Blunt
    }
    public enum Handness
    {
        Right, Left, Both, Shield
    }

    [Header("Slot")]
    public Handness handness = Handness.Right;
    public bool forbidWeapon = false;
    public bool forbidSecond = false;
    public bool forbidShield = false;
    public float load = 0f;

    [Header("Stats")]
    public WeaponItem.Type type = WeaponItem.Type.None;
    public SecondItem.Type type2 = SecondItem.Type.None;
    public Projectile projectilePrefab = null;
    [SerializeField] public int damage;
    [SerializeField] public float range;
    [SerializeField] public float rangeMinimum = 0;
    [SerializeField] public float cooldown;
    [SerializeField] public DamageType damageType;

}
