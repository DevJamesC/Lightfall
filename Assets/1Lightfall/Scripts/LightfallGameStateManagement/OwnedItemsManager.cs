using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class OwnedItemsManager : SingletonMonobehavior<OwnedItemsManager>
    {

        [SerializeField] private List<ItemType> startingItems;
        private List<ItemType> ownedItems;

        protected override void Awake()
        {
            base.Awake();

            if (startingItems == null)
                startingItems = new List<ItemType>();

            ownedItems = new List<ItemType>();
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

        public List<ItemType> GetItemsByCategory(IItemCategoryIdentifier categoryID)
        {
            List<ItemType> returnVal = new List<ItemType>();
            foreach (var item in ownedItems)
            {
                if (item.GetItemCategory().Equals(categoryID))
                    returnVal.Add(item);
            }

            return returnVal;
        }
    }
}
