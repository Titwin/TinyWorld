using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPersistance : MonoBehaviour
{
    public Dictionary<Vector3Int, Dictionary<string, int>> resourceContainersSave = new Dictionary<Vector3Int, Dictionary<string, int>>();


    public void Save(ResourceContainer container, Vector3Int position)
    {
        if (container.inventory.Count != 0)
        {
            if (!resourceContainersSave.ContainsKey(position))
                resourceContainersSave.Add(position, new Dictionary<string, int>());
            resourceContainersSave[position].Clear();
            foreach (KeyValuePair<string, int> entry in container.inventory)
                resourceContainersSave[position].Add(entry.Key, entry.Value);
        }
        else if (resourceContainersSave.ContainsKey(position))
        {
            resourceContainersSave.Remove(position);
        }
    }
}
