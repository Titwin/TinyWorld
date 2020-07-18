using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackSlot : MonoBehaviour
{
    public BackpackItem equipedItem;
    public MeshFilter equipedMesh;

    public bool Equip(BackpackItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != BackpackItem.Type.None)
        {
            BackpackItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                MeshFilter mf = newItem.GetComponent<MeshFilter>();
                if (mf)
                {
                    equipedMesh.mesh = mf.mesh;
                    BackpackItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        else
        {
            equipedItem.Clear();
            equipedMesh.mesh = null;
            return true;
        }
        return false;
    }
}
