using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName ="New InventoryItem", menuName ="Inventory")]
namespace MBS.InventorySystem
{
    public class InventoryItem : ScriptableObject
    {
        //this may change, or be uneeded depending on how the inventory is handled. 
        public void Stow()
        {
            //save item data and destroy object
        }

        public void Retrieve()
        {
            //instanciate object and apply any saved data
        }
    }
}
