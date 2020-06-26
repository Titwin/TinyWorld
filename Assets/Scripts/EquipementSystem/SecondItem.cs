using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondItem : MonoBehaviour
{
    public enum Type
    {
        None,
        LongBow, RecurveBow, ShortBow, 
        StaffA, StaffB, StaffC, StaffD
    };
    public Type type = Type.None;
    public bool forbidWeapon = false;
    public bool forbidShield = false;
    public int animationCode = 1;
    public float load = 0f;
    public float dammage = 0f;

    public void Clear()
    {
        type = Type.None;
        forbidWeapon = false;
        forbidShield = false;
        animationCode = 0;
        load = 0f;
        dammage = 0f;
    }
    public static void Copy(SecondItem source, SecondItem destination)
    {
        destination.type = source.type;
        destination.forbidWeapon = source.forbidWeapon;
        destination.forbidShield = source.forbidShield;
        destination.animationCode = source.animationCode;
        destination.load = source.load;
        destination.dammage = source.dammage;
    }
}
