/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Implements InventoryBase - adds a basic inventory to the character controller.
    /// </summary>
    public class Inventory : InventoryBase
    {
        [Serializable]
        public class AutoEquipSettings
        {
            /// <summary>
            /// Specifies the auto equip options.
            /// </summary>
            [Serializable]
            public enum Options
            {
                None,               // Do not equip anything.
                FirstLoadoutItem,   // Equip the first element in the loadout.
                ItemSetName,        // Equip the item set with the matching state name.
                ItemSetIndex        // Equip the item set at the index specified.
            }

            [Tooltip("The options for the starting item to equip.")]
            [SerializeField] private Options m_Option = Options.FirstLoadoutItem;
            [Tooltip("The Name of the item set to equip if the option was set to ItemSetName.")]
            [SerializeField] private string m_ItemSetName;
            [Tooltip("The index used if ItemSetIndex was selected as option.")]
            [SerializeField] private int m_Index;
            
            public Options Option { get => m_Option; set => m_Option = value; }
            public string ItemSetName { get => m_ItemSetName; set => m_ItemSetName = value; }
            public int Index { get => m_Index; set => m_Index = value; }

            /// <summary>
            /// Equip the item matching the fields.
            /// </summary>
            /// <param name="inventory">The inventory to equip the item on.</param>
            public void Equip(Inventory inventory)
            {
                var itemSetManager = inventory.GetComponent<ItemSetManagerBase>();
                switch (m_Option) {
                    case Options.None:
                        break;
                    case Options.FirstLoadoutItem:
                        if (inventory.DefaultLoadout.Length == 0 || inventory.DefaultLoadout[0].ItemIdentifier == null) {
                            return;
                        }

                        if (itemSetManager == null) {
                            return;
                        }

                        var itemToEquip = inventory.DefaultLoadout[0].ItemIdentifier;
                        itemSetManager.EquipItem(itemToEquip, 0, true, true);
                        break;
                    case Options.ItemSetName:
                        if (itemSetManager == null) {
                            return;
                        }
                        itemSetManager.TryEquipItemSet(m_ItemSetName, -1, true, true);
                        break;
                    case Options.ItemSetIndex:
                        if (itemSetManager == null) {
                            return;
                        }
                        var itemSet = itemSetManager.ItemSetGroups[0].GetItemSetAt(m_Index);
                        if (itemSet == null) {
                            return;
                        }
                        itemSetManager.TryEquipItemSet(itemSet, true,true);
                        break;
                    default:
                        return;
                }
            }
        }

        [Tooltip("Choose the item to equip on start.")]
        [SerializeField] protected AutoEquipSettings m_AutoEquip;
        [Tooltip("Items to load when the inventory is initially created or on a character respawn.")]
        [SerializeField] protected ItemIdentifierAmount[] m_DefaultLoadout;
        [Tooltip("The items that cannot be removed.")]
        [SerializeField] protected ItemDefinitionBase[] m_RemoveExceptions;

        private Dictionary<IItemIdentifier, CharacterItem>[] m_ItemIdentifierMap;
        private Dictionary<IItemIdentifier, int> m_ItemIdentifierAmount = new Dictionary<IItemIdentifier, int>();
        private CharacterItem[] m_ActiveItem;
        public ItemIdentifierAmount[] DefaultLoadout { get { return m_DefaultLoadout; } set { m_DefaultLoadout = value; } }
        public ItemDefinitionBase[] RemoveExceptions { get { return m_RemoveExceptions; } set { m_RemoveExceptions = value; } }
        
        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            m_ItemIdentifierMap = new Dictionary<IItemIdentifier, CharacterItem>[m_SlotCount];
            for (int i = 0; i < m_SlotCount; ++i) {
                m_ItemIdentifierMap[i] = new Dictionary<IItemIdentifier, CharacterItem>();
            }
            m_ActiveItem = new CharacterItem[m_SlotCount];
        }

        /// <summary>
        /// Add any runtime pickup items.
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();


#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
#endif
                EquipStartItem();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            }
#endif
        }

        /// <summary>
        /// Equip the default item that was set as the start item to equip.
        /// </summary>
        public virtual void EquipStartItem()
        {
            m_AutoEquip.Equip(this);
        }

        /// <summary>
        /// Pick up each ItemIdentifier within the DefaultLoadout.
        /// </summary>
        public override void LoadDefaultLoadoutInternal()
        {
            if (m_DefaultLoadout != null) {
                for (int i = 0; i < m_DefaultLoadout.Length; ++i) {
                    AddItemIdentifierAmount(m_DefaultLoadout[i].ItemIdentifier, m_DefaultLoadout[i].Amount);
                }
            }
        }

        /// <summary>
        /// Internal method which determines if the character has the specified item.
        /// </summary>
        /// <param name="characterItem">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        protected override bool HasCharacterItemInternal(CharacterItem characterItem)
        {
            if (characterItem == null) {
                return false;
            }
            return GetCharacterItem(characterItem.ItemDefinition as ItemType, characterItem.SlotID) != null;
        }

        /// <summary>
        /// Internal method which returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        protected override CharacterItem GetActiveCharacterItemInternal(int slotID)
        {
            if (slotID <= -1 || slotID >= SlotCount) {
                return null;
            }

            return m_ActiveItem[slotID];
        }

        /// <summary>
        /// Internal method which returns the item that corresponds to the specified ItemIdentifier.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <param name="characterItem">The character item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected override bool TryGetCharacterItemInternal(IItemIdentifier itemIdentifier, int slotID, out CharacterItem characterItem)
        {
            characterItem = null;
            if (itemIdentifier == null || slotID < 0 || slotID >= m_ItemIdentifierMap.Length) {
                return false;
            }

            if (m_ItemIdentifierMap[slotID].TryGetValue(itemIdentifier, out var item)) {
                characterItem = item;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Internal method which equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemIdentifier. Can be null.</returns>
        protected override CharacterItem EquipItemInternal(IItemIdentifier itemIdentifier, int slotID)
        {
            if (itemIdentifier == null || slotID < -1 || slotID >= SlotCount) {
                return null;
            }

            // The ItemIdentifier has to exist in the inventory.
            if (!m_ItemIdentifierMap[slotID].TryGetValue(itemIdentifier, out var item)) {
                Debug.LogError($"Error: Unable to equip item with ItemIdentifier {itemIdentifier}: the itemIdentifier hasn't been added to the inventory.");
                return null;
            }

            m_ActiveItem[slotID] = item;
            return item;
        }

        /// <summary>
        /// Internal method which unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was unequipped.</returns>
        protected override CharacterItem UnequipItemInternal(int slotID)
        {
            if (slotID < -1 || slotID >= SlotCount) {
                return null;
            }

            var prevItem = m_ActiveItem[slotID];
            m_ActiveItem[slotID] = null;
            return prevItem;
        }

        /// <summary>
        /// Internal method which returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <param name="includeExternalItems">Should the external items be included in the search?</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        protected override int GetItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, bool includeExternalItems)
        {
            m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var amount);
            return amount;
        }

        /// <summary>
        /// Add an item identifier amount.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to add.</param>
        /// <param name="amount">The amount to add.</param>
        protected override void AddItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount)
        {
            var capacity = int.MaxValue;
            if (itemIdentifier is ItemType itemType) {
                capacity = itemType.Capacity;
            }
            
            if (!m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var existingAmount)) {
                m_ItemIdentifierAmount[itemIdentifier] = Mathf.Min(amount, capacity);
            } else {
                m_ItemIdentifierAmount[itemIdentifier] = Mathf.Min(amount + existingAmount, capacity);
            }
        }

        /// <summary>
        /// Remove the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="amount">The amount of ItemIdentifier to remove.</param>
        protected override void RemoveItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount)
        {
            if (!m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var existingAmount)) {
                return;
            }

            var newAmount= existingAmount - amount;
            if (newAmount <= 0) {
                m_ItemIdentifierAmount.Remove(itemIdentifier);
            } else {
                m_ItemIdentifierAmount[itemIdentifier] = newAmount;
            }
        }

        /// <summary>
        /// Can the item be removed?
        /// </summary>
        /// <param name="itemIdentifier">The item to remove.</param>
        /// <returns>True if it can be removed.</returns>
        protected override bool CanRemoveItemIdentifier(IItemIdentifier itemIdentifier)
        {
            if (m_RemoveExceptions == null) {
                return true;
            }

            for (int i = 0; i < m_RemoveExceptions.Length; i++) {
                if (m_RemoveExceptions[i] == itemIdentifier.GetItemDefinition()) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the Character Item Prefabs for the item identifier.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier from which the character item prefabs should be retrieved.</param>
        /// <returns>A list slice of character item prefabs as GameObjects.</returns>
        public override ListSlice<GameObject> GetItemIdentifierCharacterItemPrefabs(IItemIdentifier itemIdentifier)
        {
            if (itemIdentifier is ItemType itemType) {
                return itemType.GetPrefabs();
            }

            return (null, 0, 0);
        }

        /// <summary>
        /// When a character item is spawned send events to notify objects outside the inventory.
        /// </summary>
        /// <param name="characterItem">The character Item that was added.</param>
        public override void OnCharacterItemSpawned(CharacterItem characterItem)
        {
            base.OnCharacterItemSpawned(characterItem);
            m_ItemIdentifierMap[characterItem.SlotID][characterItem.ItemIdentifier] = characterItem;
        }

        /// <summary>
        /// A Character item will be destroyed, remove it from the dictionaries and lists.
        /// </summary>
        /// <param name="characterItem">The character item that will be destroyed.</param>
        protected override void OnCharacterItemWillBeDestroyed(CharacterItem characterItem)
        {
            base.OnCharacterItemWillBeDestroyed(characterItem);
            m_ItemIdentifierMap[characterItem.SlotID].Remove(characterItem.ItemIdentifier);
        }
        
        /// <summary>
        /// The character has respawned.
        /// </summary>
        protected override void OnRespawn()
        {
            base.OnRespawn();
            EquipRespawnItem();
        }

        /// <summary>
        /// Equip the items after respawn.
        /// </summary>
        protected virtual void EquipRespawnItem()
        {
            if(m_LoadDefaultLoadoutOnRespawn == false){ return; }
            m_AutoEquip.Equip(this);
        }
    }
}