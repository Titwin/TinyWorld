using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BackpackItem : Item
{
    public enum Type
    {
        None,
        QuiverA,
        QuiverB,
        RessourceContainer
    };

    [Header("Backpack Item")]
    public Type type = Type.None;
    public string toolFamily = "None";

    

    public void Clear()
    {
        type = Type.None;
        toolFamily = "None";
        load = 0f;
    }
    public static void Copy(BackpackItem source, BackpackItem destination)
    {
        Item.Copy(source, destination);
        destination.type = source.type;
        destination.load = source.load;
        destination.toolFamily = source.toolFamily;
    }
}
