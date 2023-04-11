using Opsive.Shared.Events;
using Opsive.Shared.Input;
using Opsive.Shared.Integrations.InputSystem;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MBS.Lightfall
{
    public class ActivateLoadoutUI : CharacterMonitor, IInteractableTarget, IInteractableMessage
    {
        protected UnityEngine.InputSystem.PlayerInput playerInput;
        protected UnityInputSystem opsiveUnityInput;

        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);

            if (character != null)
            {
                PlayerInputProxy inputProxy = character.GetComponentInChildren<PlayerInputProxy>();
                playerInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                opsiveUnityInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityInputSystem>();
            }
        }

        public string AbilityMessage()
        {
            InputControl inputControl = null;
            if (playerInput.currentControlScheme == "Keyboard")
                inputControl = Keyboard.current;
            if (playerInput.currentControlScheme == "Gamepad")
                inputControl = Gamepad.current;

            InputAction action = playerInput.actions.FindAction("Action");
            string interactKey = action.GetBindingDisplayString(InputBinding.MaskByGroup(playerInput.currentControlScheme));

            return $"{interactKey} to Change Loadout";
        }

        public bool CanInteract(GameObject character)
        {
            if (playerInput.currentActionMap.name == "Gameplay")
                return true;

            return false;
        }

        public void Interact(GameObject character)
        {
            EventHandler.ExecuteEvent("OpenEquipmentLoadoutUI");
        }

    }
}
