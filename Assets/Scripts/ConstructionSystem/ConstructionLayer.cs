using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConstructionLayer : MonoBehaviour
{
    public enum LayerType
    {
        Terrain,
        Building,
        Decoration,
        Delete
    }
    public LayerType layerType;
    public List<ConstructionData> elements = new List<ConstructionData>();

    public LayerMask ToLayerMask()
    {
        return LayerMask.NameToLayer(layerType.ToString());
    }
}
