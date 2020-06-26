using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseSlot : MonoBehaviour
{
    public HorseItem equipedItem;
    public SkinnedMeshRenderer equipedMesh;

    public bool Equip(HorseItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != HorseItem.Type.None)
        {
            HorseItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                SkinnedMeshRenderer smr = newItem.GetComponent<SkinnedMeshRenderer>();
                if (smr)
                {
                    CopySkinnedMesh(smr, equipedMesh);
                    HorseItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        equipedItem.Clear();
        equipedMesh.sharedMesh = null;
        return false;
    }

    public static void CopySkinnedMesh(SkinnedMeshRenderer source, SkinnedMeshRenderer destination)
    {
        destination.sharedMesh = source.sharedMesh;
        Dictionary<string, Transform> remap = new Dictionary<string, Transform>();
        for (int i = 0; i < destination.bones.Length; i++)
            remap[destination.bones[i].name] = destination.bones[i];

        Transform[] newBoneList = new Transform[source.bones.Length];
        for (int i = 0; i < source.bones.Length; i++)
            newBoneList[i] = remap[source.bones[i].name];

        destination.bones = newBoneList;
        destination.sharedMesh = source.sharedMesh;
    }
}
