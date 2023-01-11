using MBS.Lightfall;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Traits;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartWave : MonoBehaviour, IInteractableTarget, IInteractableMessage
{
    public string AbilityMessage()
    {
        return "Press F to Start Wave";
    }

    public bool CanInteract(GameObject character)
    {
        return true;
    }

    public void Interact(GameObject character)
    {
        EventHandler.ExecuteEvent(WaveManager.Instance.gameObject, "OnWaveStart");
    }
}
