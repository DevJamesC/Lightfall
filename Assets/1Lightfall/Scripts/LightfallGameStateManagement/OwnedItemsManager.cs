using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class OwnedItemsManager : SingletonMonobehavior<OwnedItemsManager>
    {
        [SerializeField,Tooltip("Empty Item should not have an item type")] private ItemDetailsScriptableObject EmptyItem;
        [SerializeField] private List<ItemDetailsScriptableObject> startingItems;
        private List<ItemDetailsScriptableObject> ownedItems;

        protected override void Awake()
        {
            base.Awake();

            if (startingItems == null)
                startingItems = new List<ItemDetailsScriptableObject>();

            ownedItems = new List<ItemDetailsScriptableObject>();
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (var item in startingItems)
            {
                if (!ownedItems.Contains(item))
                    ownedItems.Add(item);
            }
        }

        public List<ItemDetailsScriptableObject> GetItemsByCategory(IItemCategoryIdentifier categoryID, bool includeEmpty)
        {
            List<ItemDetailsScriptableObject> returnVal = new List<ItemDetailsScriptableObject>();

            if (includeEmpty)
                returnVal.Add(EmptyItem);

            foreach (var item in ownedItems)
            {
                if (item.ItemType.GetItemCategory().Equals(categoryID))
                    returnVal.Add(item);
            }

            return returnVal;
        }
    }
}
