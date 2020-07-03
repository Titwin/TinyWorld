using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldItem : Item
{
    public enum Type
    {
        None,
        RoundA, RoundB,
        CavaleryA, CavaleryB, CavaleryC, CavaleryD, CavaleryE, CavaleryF, CavaleryG,
        RectangleA, RectangleB, RectangleC,
        KiteA, KiteB, KiteC, KiteD, KiteE, KiteF, KiteG, KiteH, KiteI, KiteJ, KiteK
    };

    [Header("Shield Item")]
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



    public static void Copy(ShieldItem source, ShieldItem destination)
    {
        Item.Copy(source, destination);
        destination.type = source.type;
        destination.load = source.load;
        destination.armor = source.armor;
    }
}
