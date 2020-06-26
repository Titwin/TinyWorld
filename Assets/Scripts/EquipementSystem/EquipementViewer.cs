using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipementViewer : MonoBehaviour
{
    [Header("Debug")]
    public bool visible = false;
    
    [Header("Links")]
    public GameObject pivot;
    public PlayerController player;

    public EquipementLineTemplate weapon;
    public EquipementLineTemplate secondaryWeapon;
    public EquipementLineTemplate body;
    public EquipementLineTemplate head;
    public EquipementLineTemplate shield;
    public EquipementLineTemplate backpack;
    public EquipementLineTemplate total;

    private void Start()
    {
        UpdateContent();
        pivot.SetActive(visible);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            visible = !visible;
            pivot.SetActive(visible);
            if (visible)
                UpdateContent();
        }
    }

    public void UpdateContent()
    {
        float dammage = 0f;
        float weight = 0f;
        float armor = 0f;

        // weapon
        weapon.armor.text = "0";
        if (player.weapon.equipedItem.type == WeaponItem.Type.None)
        {
            weapon.dammage.text = "1";
            weapon.weight.text = "0";
            dammage += 1f;
        }
        else
        {
            weight += player.weapon.equipedItem.load;
            dammage += player.weapon.equipedItem.dammage;
            weapon.dammage.text = player.weapon.equipedItem.dammage.ToString();
            weapon.weight.text = player.weapon.equipedItem.load.ToString();
        }

        // second weapon
        secondaryWeapon.armor.text = "0";
        if (player.secondHand.equipedItem.type == SecondItem.Type.None)
        {
            secondaryWeapon.dammage.text = "0";
            secondaryWeapon.weight.text = "0";
        }
        else
        {
            weight += player.secondHand.equipedItem.load;
            dammage += player.secondHand.equipedItem.dammage;
            secondaryWeapon.dammage.text = player.secondHand.equipedItem.dammage.ToString();
            secondaryWeapon.weight.text = player.secondHand.equipedItem.load.ToString();
        }

        // body
        body.dammage.text = "0";
        if (player.body.equipedItem.type == BodyItem.Type.None)
        {
            body.armor.text = "0";
            body.weight.text = "0";
        }
        else
        {
            weight += player.body.equipedItem.load;
            armor += player.body.equipedItem.armor;
            body.armor.text = player.body.equipedItem.armor.ToString();
            body.weight.text = player.body.equipedItem.load.ToString();
        }

        // head
        head.dammage.text = "0";
        if (player.head.equipedItem.type == HeadItem.Type.None)
        {
            head.armor.text = "0";
            head.weight.text = "0";
        }
        else
        {
            weight += player.head.equipedItem.load;
            armor += player.head.equipedItem.armor;
            head.armor.text = player.head.equipedItem.armor.ToString();
            head.weight.text = player.head.equipedItem.load.ToString();
        }

        // shield
        shield.dammage.text = "0";
        if (player.shield.equipedItem.type == ShieldItem.Type.None)
        {
            shield.armor.text = "0";
            shield.weight.text = "0";
        }
        else
        {
            weight += player.shield.equipedItem.load;
            armor += player.shield.equipedItem.armor;
            shield.armor.text = player.shield.equipedItem.armor.ToString();
            shield.weight.text = player.shield.equipedItem.load.ToString();
        }

        // backpack
        backpack.dammage.text = "0";
        backpack.armor.text = "0";
        if (player.backpack.equipedItem.type == BackpackItem.Type.None)
        {
            backpack.weight.text = "0";
        }
        else
        {
            weight += player.backpack.equipedItem.load;
            backpack.weight.text = player.backpack.equipedItem.load.ToString();
        }

        // total
        total.armor.text = armor.ToString();
        total.dammage.text = dammage.ToString();
        total.weight.text = weight.ToString();
    }
}
