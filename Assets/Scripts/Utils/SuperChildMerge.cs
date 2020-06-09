using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SuperChildMerge : MonoBehaviour
{
    public void Start()
    {
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combine = new List<CombineInstance>();

        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].sharedMesh)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = meshFilters[i].sharedMesh;
                ci.transform = meshFilters[i].transform.localToWorldMatrix;
                combine.Add(ci);
            }
            i++;
        }

        gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().sharedMesh.name = this.name;
        gameObject.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine.ToArray());
        transform.gameObject.SetActive(true);
        
        foreach(Transform t in transform)
        {
            foreach (BoxCollider b in t.GetComponents<BoxCollider>())
            {
                BoxCollider newb = gameObject.AddComponent<BoxCollider>();
                newb.center = t.TransformPoint(b.center);
                Vector3 v = t.TransformDirection(b.size);
                newb.size = new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
                newb.isTrigger = b.isTrigger;
            }
            Destroy(t.gameObject);
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;
        gameObject.SetActive(false);
        Destroy(this);
    }
}
