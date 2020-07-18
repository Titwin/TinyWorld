using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipementSFXManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponMapping
    {
        public WeaponItem.Type type;
        public GameObject associated;
    }

    public WeaponItem weapon;
    public List<WeaponMapping> mappingList = new List<WeaponMapping>();
    public Dictionary<WeaponItem.Type, GameObject> mapping = new Dictionary<WeaponItem.Type, GameObject>();
    private WeaponItem.Type lastType = WeaponItem.Type.None;

    private void Start()
    {
        foreach (WeaponMapping wm in mappingList)
        {
            mapping.Add(wm.type, wm.associated);
            wm.associated.SetActive(false);
        }
    }

    void Update()
    {
        if (weapon)
        {
            if (weapon.type != lastType)
            {
                if (mapping.ContainsKey(lastType))
                    mapping[lastType].SetActive(false);
                if (mapping.ContainsKey(weapon.type))
                    mapping[weapon.type].SetActive(true);
            }
            lastType = weapon.type;
        }
        else
        {
            weapon = transform.parent.gameObject.GetComponent<WeaponItem>();
        }
    }
}
