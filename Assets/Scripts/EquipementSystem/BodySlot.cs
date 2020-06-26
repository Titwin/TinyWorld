using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySlot : MonoBehaviour
{
    public BodyItem equipedItem;
    public SkinnedMeshRenderer equipedMesh;

    public bool Equip(BodyItem.Type type, bool mounted, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != BodyItem.Type.None)
        {
            BodyItem newItem = Arsenal.Instance.Get(type, mounted);
            if (newItem)
            {
                SkinnedMeshRenderer smr = newItem.GetComponent<SkinnedMeshRenderer>();
                if (smr)
                {
                    CopySkinnedMesh(smr, equipedMesh);
                    BodyItem.Copy(newItem, equipedItem);
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
