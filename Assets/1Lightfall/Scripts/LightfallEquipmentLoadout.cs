using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class LightfallEquipmentLoadout : MonoBehaviour
    {
        private ItemType primaryEquipment;
        private ItemType secondaryEquipment;
        private ItemType armor;

        private Inventory inventory;
        private ItemSetManager itemSetManager;
        //Reference to the actual prefabs
        private CharacterItem primaryEquipmentItem;
        private CharacterItem secondaryEquipmentItem;
        private CharacterItem armorItem;

        public ItemType PrimaryEquipment
        {
            get { return primaryEquipment; }
        }
        public ItemType SecondaryEquipment
        {
            get { return secondaryEquipment; }
        }
        public ItemType Armor { get { return armor; } }




        private void Awake()
        {
            inventory = GetComponent<Inventory>();
            itemSetManager = GetComponent<ItemSetManager>();
        }

        public void ChangeLoadoutEquipment(ItemType newItemType, LoadoutEquipmentType loadoutEquipmentType)
        {
            if (loadoutEquipmentType == LoadoutEquipmentType.Primary || loadoutEquipmentType == LoadoutEquipmentType.Secondary)
            {
                //check that this equipment is not in our secondary or primary slot
                if (newItemType == secondaryEquipment)
                    return;
                if (newItemType == primaryEquipment)
                    return;

                //remove current primaryEquipment from inventory
                CharacterItem itemToRemove = loadoutEquipmentType == LoadoutEquipmentType.Primary ? primaryEquipmentItem : secondaryEquipmentItem;
                if (itemToRemove != null)
                {
                    inventory.RemoveCharacterItem(itemToRemove, true);
                }

                //add new equipment to inventory
                if (newItemType != null)
                {
                    inventory.AddItemIdentifierAmount(newItemType, 1);
                }
            }
            else if (loadoutEquipmentType != LoadoutEquipmentType.Armor)
            {
                Debug.Log("armor...");
            }

        }




        public enum LoadoutEquipmentType
        {
            Primary,
            Secondary,
            Armor
        }

    }
}
