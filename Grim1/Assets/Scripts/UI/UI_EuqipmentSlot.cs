using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EuqipmentSlot : UI_ItemSlot
{
    public EquipmentType slotType;

    private void OnValidate()
    {
        gameObject.name= "Euqipment slot - " + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Inventory.Instance.UnequipItem(item.data as ItemData_Equipment);
        Inventory.Instance.AddItem(item.data as ItemData_Equipment);
        CleanUpSlot();
    }
}