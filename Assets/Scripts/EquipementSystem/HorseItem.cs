using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseItem : Item
{
    public enum Type
    {
        None,
        HorseA, HorseB, HorseC, HorseD, HorseE, HorseF, HorseG, HorseH
    };

    [Header("Horse Item")]
    public Type type = Type.None;
    public float armor = 0f;

    public void Clear()
    {
        type = Type.None;
        load = 0f;
        armor = 0f;
    }
    public override SummarizedItem Summarize()
    {
        SummarizedItem sumItem = base.Summarize();
        sumItem.derivatedType = (int)type;
        return sumItem;
    }


    public static void Copy(HorseItem source, HorseItem destination)
    {
        Item.Copy(source, destination);
        destination.type = source.type;
        destination.load = source.load;
        destination.armor = source.armor;
    }
}
