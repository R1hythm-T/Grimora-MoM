using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EuqipmentSlot : UI_ItemSlot
{
    public EquipmentType slotType;

    private void OnValidate()
    {
        gameObject.name= "Euqipment slot - " + slotType.ToString();
    }
}