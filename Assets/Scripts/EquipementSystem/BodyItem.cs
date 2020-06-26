using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyItem : MonoBehaviour
{
    public enum Type
    {
        None,
        NeckedA, NeckedB, NeckedC,
        DressA, DressB, DressC, DressD, DressE,
        CivilianA, CivilianB, CivilianC, CivilianD, CivilianE,
        LeatherA, LeatherB, LeatherC, LeatherD,
        LightA, LightB, LightC, LightD, LightE, LightF, LightG, LightH,
        MediumA, MediumB, MediumC, MediumD, MediumE, MediumF, MediumG, MediumH,
        HeavyA, HeavyB, HeavyC, HeavyD, HeavyE, HeavyF, HeavyG, HeavyH, HeavyI,
        NobleA, NobleB,
        MediumI
    };
    public enum Category
    {
        Cloth, Light, Medium, Heavy
    };
    public Type type = Type.None;
    public float load = 0f;
    public float armor = 0f;

    public void Clear()
    {
        type = Type.None;
        load = 0f;
        armor = 0f;
    }
    public static void Copy(BodyItem source, BodyItem destination)
    {
        destination.type = source.type;
        destination.load = source.load;
        destination.armor = source.armor;
    }

    // helper for interaction system
    static public Category getCategory(Type type)
    {
        switch(type)
        {
            case Type.LeatherA:
            case Type.LeatherB:
            case Type.LeatherC:
            case Type.LeatherD:
            case Type.LightA:
            case Type.LightB:
            case Type.LightC:
            case Type.LightD:
            case Type.LightE:
            case Type.LightF:
            case Type.LightG:
            case Type.LightH:
                return Category.Light;
            case Type.MediumA:
            case Type.MediumB:
            case Type.MediumC:
            case Type.MediumD:
            case Type.MediumE:
            case Type.MediumF:
            case Type.MediumG:
            case Type.MediumH:
            case Type.MediumI:
                return Category.Medium;
            case Type.HeavyA:
            case Type.HeavyB:
            case Type.HeavyC:
            case Type.HeavyD:
            case Type.HeavyE:
            case Type.HeavyF:
            case Type.HeavyG:
            case Type.HeavyH:
            case Type.HeavyI:
                return Category.Heavy;
            default:
                return Category.Cloth;
        }
    }
}
