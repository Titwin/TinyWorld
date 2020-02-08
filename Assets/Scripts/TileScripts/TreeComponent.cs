﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeComponent : MonoBehaviour
{
    public SkinnedMeshRenderer snow;
    public SkinnedMeshRenderer[] leaves;
    public BillboardRenderer billboardRenderer;

    public BillboardAsset snowBillboard;
    public BillboardAsset barkBillboard;
    public BillboardAsset leaveBillboard;

    public void SetConfiguration(bool hasLeaves, bool hasSnow)
    {
        if (snow)
            snow.enabled = hasSnow;
        foreach (SkinnedMeshRenderer leave in leaves)
            leave.enabled = hasLeaves;

        if(hasSnow)
        {
            billboardRenderer.billboard = snowBillboard;
        }
        else if(hasLeaves)
        {
            billboardRenderer.billboard = leaveBillboard;
        }
        else
        {
            billboardRenderer.billboard = barkBillboard;
        }
    }
}
