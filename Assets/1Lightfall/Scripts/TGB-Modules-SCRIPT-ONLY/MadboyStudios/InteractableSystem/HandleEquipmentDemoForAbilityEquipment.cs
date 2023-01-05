using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.InteractionSystem
{
    public class HandleEquipmentDemoForAbilityEquipment : MonoBehaviour
    {
        [SerializeField]
        private Transform handPosition;
        [SerializeField, ReadOnly]
        private EquippableBase currentEquippedObj;

        public EquippableBase Equip(EquippableBase newEquippable)
        {
            UnEquip();

            currentEquippedObj = Instantiate(newEquippable, handPosition);
            currentEquippedObj.Equip(this);

            //returns a reference so methods that call Equip can get a reference to the newly equipped object
            return currentEquippedObj;
        }

        internal void UnEquip()
        {

            if (currentEquippedObj != null)
            {
                currentEquippedObj.Unequip(this);
            }
        }
    }
}
