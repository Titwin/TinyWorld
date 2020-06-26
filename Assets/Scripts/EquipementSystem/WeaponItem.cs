using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    public enum Type
    {
        None,
        AxeA, AxeB, AxeC, AxeD, AxeE, BigAxeA, BigAxeB,
        Bardiche,
        BroadSwordA, BroadSwordB, ShortSwordA, ShortSwordB, SwordA, SwordB, BigSwordA, BigSwordB,
        Club, BigClub,
        CrossbowA, CrossbowB,
        DaggerA, DaggerB, DaggerC,
        Glaive,
        Halberd,
        Hammer, Warhammer, BigWarhammerA, BigWarhammerB,
        MaceA, MaceB, 
        MaulA, MaulB, BigMaulA, BigMaulB,
        MorningStar,
        Pickaxe,
        Pike,
        Sabre,
        Claymore,
        Spear,

        Sickle,

        CavalryLanceA, CavalryLanceB, CavalryLanceC,
        CavalrySpear
    };
    public Type type = Type.None;
    public string toolFamily = "None";
    public bool forbidSecond = false;
    public bool forbidShield = false;
    public int animationCode = 1;
    public float load = 0f;
    public float dammage = 0f;
    
    // special
    public void Clear()
    {
        type = Type.None;
        toolFamily = "None";
        forbidSecond = false;
        forbidShield = false;
        animationCode = 0;
        load = 0f;
        dammage = 0f;
    }
    public static void Copy(WeaponItem source, WeaponItem destination)
    {
        destination.type = source.type;
        destination.toolFamily = source.toolFamily;
        destination.forbidSecond = source.forbidSecond;
        destination.forbidShield = source.forbidShield;
        destination.animationCode = source.animationCode;
        destination.load = source.load;
        destination.dammage = source.dammage;
    }
}
