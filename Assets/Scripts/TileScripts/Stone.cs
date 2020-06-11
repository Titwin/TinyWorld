﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int size;
    public int yieldPerSize = 4;

    private GameObject stone = null;

    public void Initialize(int rockSize)
    {
        stone = ObjectPooler.instance.get(GetPrefabName(rockSize));
        stone.transform.parent = transform;
        stone.transform.localPosition = Vector3.zero;
        stone.transform.localScale = Vector3.one;
        stone.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        int dispersion = (int)(0.25f * (rockSize + 1) * yieldPerSize);
        stone.transform.Find("Interactor").GetComponent<CollectData>().ressourceCount = Random.Range((rockSize + 1) * yieldPerSize - dispersion, (rockSize + 1) * yieldPerSize + dispersion);
    }

    private string GetPrefabName(int size)
    {
        size = Mathf.Clamp(size, 0, 2);
        switch (size)
        {
            case 0: return "SmallStone";
            case 1: return "MidStone";
            case 2: return "BigStone";
            default: return "SmallStone";
        }
    }

    private void OnDestroy()
    {
        if (ObjectPooler.instance.ContainTag(stone.name))
            ObjectPooler.instance.free(stone);
        else Destroy(stone);
    }
}
