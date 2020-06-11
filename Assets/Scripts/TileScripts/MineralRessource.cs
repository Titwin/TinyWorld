using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralRessource : MonoBehaviour
{
    public void Initialize(Material m)
    {
        GameObject go = TilePrefabsContainer.Instance.GetOre(m.name);
        go.name = "stone";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        go.SetActive(true);
    }
}
