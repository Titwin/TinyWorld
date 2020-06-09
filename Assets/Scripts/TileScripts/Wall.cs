using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public int configuration;

    public void Initialize(bool xp, bool xm, bool zp, bool zm, string tileName)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        float rotation = 0f;
        GameObject prefab = null;

        List<GameObject> prefabs;

        if (tileName.Contains("Stone"))
            prefabs = ConstructionDictionary.Instance.wallPrefabsStone;
        else prefabs = ConstructionDictionary.Instance.wallPrefabsWood;

        switch (configuration)
        {
            case 0:
                prefab = prefabs[0];
                rotation = 0f;
                break;
            case 1:
                prefab = prefabs[1];
                rotation = 0f;
                break;
            case 2:
                prefab = prefabs[1];
                rotation = 180f;
                break;
            case 3:
                prefab = prefabs[2];
                rotation = 0f;
                break;
            case 4:
                prefab = prefabs[1];
                rotation = 90f;
                break;
            case 5:
                prefab = prefabs[3];
                rotation = 0f;
                break;
            case 6:
                prefab = prefabs[3];
                rotation = 90f;
                break;
            case 7:
                prefab = prefabs[4];
                rotation = 90f;
                break;
            case 8:
                prefab = prefabs[1];
                rotation = -90f;
                break;
            case 9:
                prefab = prefabs[3];
                rotation = -90f;
                break;
            case 10:
                prefab = prefabs[3];
                rotation = -180f;
                break;
            case 11:
                prefab = prefabs[4];
                rotation = -90f;
                break;
            case 12:
                prefab = prefabs[2];
                rotation = 90f;
                break;
            case 13:
                prefab = prefabs[4];
                rotation = 0f;
                break;
            case 14:
                prefab = prefabs[4];
                rotation = 180f;
                break;
            case 15:
                prefab = prefabs[5];
                rotation = 0f;
                break;
            default:
                Debug.LogError("Wall init : invald tile configuration");
                break;
        }

        // end
        GameObject go = Instantiate(prefab);
        go.name = "building";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
    }
}
