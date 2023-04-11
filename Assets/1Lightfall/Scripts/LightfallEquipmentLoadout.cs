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
        [SerializeField] private ItemDetailsScriptableObject startingPrimaryItem;
        [SerializeField] private ItemDetailsScriptableObject startingSecondaryItem;

        private ItemDetailsScriptableObject primaryEquipment;
        private ItemDetailsScriptableObject secondaryEquipment;
        private ItemDetailsScriptableObject armor;

        private Inventory inventory;
        private ItemSetManager itemSetManager;
        //Reference to the actual prefabs
        private CharacterItem primaryEquipmentItem;
        private CharacterItem secondaryEquipmentItem;
        private CharacterItem armorItem;

        public ItemDetailsScriptableObject PrimaryEquipment
        {
            get { return primaryEquipment; }
        }
        public ItemDetailsScriptableObject SecondaryEquipment
        {
            get { return secondaryEquipment; }
        }
        public ItemDetailsScriptableObject Armor { get { return armor; } }




        private void Awake()
        {
            inventory = GetComponent<Inventory>();
            itemSetManager = GetComponent<ItemSetManager>();
        }

        private void Start()
        {
            if (startingPrimaryItem != null)
                ChangeLoadoutEquipment(startingPrimaryItem, LoadoutEquipmentType.Primary);
            if (startingSecondaryItem != null)
                ChangeLoadoutEquipment(startingSecondaryItem, LoadoutEquipmentType.Secondary);

        }

        public void ChangeLoadoutEquipment(ItemDetailsScriptableObject newItemType, LoadoutEquipmentType loadoutEquipmentType)
        {
            //if the item is empty, clear the slot and return
            if (loadoutEquipmentType == LoadoutEquipmentType.Primary || loadoutEquipmentType == LoadoutEquipmentType.Secondary)
            {
                if (newItemType.ItemType == null)
                {

                    if (loadoutEquipmentType == LoadoutEquipmentType.Primary)
                    {
                        if (primaryEquipmentItem != null)
                            inventory.RemoveCharacterItem(primaryEquipmentItem, true);
                        primaryEquipment = null;
                    }

                    if (loadoutEquipmentType == LoadoutEquipmentType.Secondary)
                    {
                        if (secondaryEquipmentItem != null)
                            inventory.RemoveCharacterItem(secondaryEquipmentItem, true);
                        secondaryEquipment = null;
                    }

                    return;
                }


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
                    inventory.AddItemIdentifierAmount(newItemType.ItemType, 1);

                    if (loadoutEquipmentType == LoadoutEquipmentType.Primary)
                        primaryEquipmentItem = inventory.GetCharacterItem(newItemType.ItemType);
                    if (loadoutEquipmentType == LoadoutEquipmentType.Secondary)
                        secondaryEquipmentItem = inventory.GetCharacterItem(newItemType.ItemType);
                }
                if (loadoutEquipmentType == LoadoutEquipmentType.Primary)
                    primaryEquipment = newItemType;
                if (loadoutEquipmentType == LoadoutEquipmentType.Secondary)
                    secondaryEquipment = newItemType;
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
