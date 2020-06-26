using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseItem : MonoBehaviour
{
    public enum Type
    {
        None,
        HorseA, HorseB, HorseC, HorseD, HorseE, HorseF, HorseG, HorseH
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
    public static void Copy(HorseItem source, HorseItem destination)
    {
        destination.type = source.type;
        destination.load = source.load;
        destination.armor = source.armor;
    }
}
