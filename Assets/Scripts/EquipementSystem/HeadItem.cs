using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadItem : MonoBehaviour
{
    public enum Type
    {
        None,
        NackedA, NackedB, NackedC, NackedD, NackedE,
        HeadbandA, HeadbandB, HeadbandC, HeadbandD, HeadbandE, HeadbandF,
        HoodA, HoodB, HoodC, HoodD, HoodE,
        ChainmailA, ChainmailB, ChainmailC, ChainmailD, ChainmailE, ChainmailF,
        SpangenhelmA, SpangenhelmB, SpangenhelmC,
        SalletA, SalletB, SalletC, SalletD, SalletE, SalletF, SalletG,
        HornA, HornB, HornC, HornD, HornE, HornF, HornG, HornH, HornI,

        SalletH, SalletI, SalletJ,
        BacinetA, BacinetB, BacinetC, BacinetD, BacinetE,
        HelmA, HelmB, HelmC, HelmD, HelmE, HelmF, HelmG, HelmH, HelmI, HelmJ,
        HatA, HatB, HatC, HatD, HatE,
        CrownA, CrownB, CrownC, CrownD, CrownE,
        HelmK, HelmL
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
    public static void Copy(HeadItem source, HeadItem destination)
    {
        destination.type = source.type;
        destination.load = source.load;
        destination.armor = source.armor;
    }

    // helper for interaction system
    static public Category getCategory(Type type)
    {
        switch (type)
        {
            case Type.ChainmailA:
            case Type.ChainmailB:
            case Type.ChainmailC:
            case Type.ChainmailD:
            case Type.ChainmailE:
            case Type.ChainmailF:
                return Category.Light;
            case Type.SpangenhelmA:
            case Type.SpangenhelmB:
            case Type.SpangenhelmC:
            case Type.SalletA:
            case Type.SalletB:
            case Type.SalletC:
            case Type.SalletD:
            case Type.SalletE:
            case Type.SalletF:
            case Type.SalletG:
            case Type.SalletH:
            case Type.SalletI:
            case Type.SalletJ:


            case Type.BacinetA:
            case Type.BacinetB:
            case Type.BacinetC:
            case Type.BacinetD:
            case Type.BacinetE:
            case Type.HelmA:
            case Type.HelmB:
            case Type.HelmC:
            case Type.HelmD:
            case Type.HelmE:
            case Type.HelmF:
            case Type.HelmG:
            case Type.HelmH:
            case Type.HelmI:
            case Type.HelmJ:
            case Type.HelmK:
            case Type.HelmL:
            case Type.HornA:
            case Type.HornB:
            case Type.HornC:
            case Type.HornD:
            case Type.HornE:
            case Type.HornF:
            case Type.HornG:
            case Type.HornH:
            case Type.HornI:
            case Type.CrownC:
            case Type.CrownD:
            case Type.CrownE:
                return Category.Heavy;
            default:
                return Category.Cloth;
        }
    }
}
