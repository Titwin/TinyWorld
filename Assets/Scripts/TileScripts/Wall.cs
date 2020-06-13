using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IPoolableObject
{
    //public int configuration;
    private GameObject wall = null;

    public void Initialize(bool xp, bool xm, bool zp, bool zm, string tileName)
    {
        if (wall) ObjectPooler.instance.Free(wall);
        
        int configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        float rotation = 0f;
        
        string poolName;
        if (tileName.Contains("Stone"))
            poolName = "StoneWall_";
        else poolName = "WoodWall_";

        switch (configuration)
        {
            case 0:
                poolName += "A";
                rotation = 0f;
                break;
            case 1:
                poolName += "B";
                rotation = 0f;
                break;
            case 2:
                poolName += "B";
                rotation = 180f;
                break;
            case 3:
                poolName += "C";
                rotation = 0f;
                break;
            case 4:
                poolName += "B";
                rotation = 90f;
                break;
            case 5:
                poolName += "D";
                rotation = 0f;
                break;
            case 6:
                poolName += "D";
                rotation = 90f;
                break;
            case 7:
                poolName += "E";
                rotation = 90f;
                break;
            case 8:
                poolName += "B";
                rotation = -90f;
                break;
            case 9:
                poolName += "D";
                rotation = -90f;
                break;
            case 10:
                poolName += "D";
                rotation = -180f;
                break;
            case 11:
                poolName += "E";
                rotation = -90f;
                break;
            case 12:
                poolName += "C";
                rotation = 90f;
                break;
            case 13:
                poolName += "E";
                rotation = 0f;
                break;
            case 14:
                poolName += "E";
                rotation = 180f;
                break;
            case 15:
                poolName += "F";
                rotation = 0f;
                break;
            default:
                Debug.LogError("Wall init : invald tile configuration");
                break;
        }
        
        wall = ObjectPooler.instance.Get(poolName);
        wall.transform.parent = transform;
        wall.transform.localPosition = Vector3.zero;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = Vector3.one;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
    }

    public void OnFree()
    {
        ObjectPooler.instance.Free(wall);
        wall = null;
    }

    public void OnInit()
    {
        
    }

    public void OnReset()
    {
        OnFree();
    }
}
