using Opsive.UltimateCharacterController.Traits;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateLoadoutUI : MonoBehaviour,IInteractableTarget,IInteractableMessage
{
    public string AbilityMessage()
    {
        return "Change Loadout";
    }

    public bool CanInteract(GameObject character)
    {
        return true;
    }

    public void Interact(GameObject character)
    {
        Debug.Log("Inventory Activated");
    }

    
}
