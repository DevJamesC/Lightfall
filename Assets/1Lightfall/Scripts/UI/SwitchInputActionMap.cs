using Opsive.Shared.Input;
using Opsive.Shared.Integrations.InputSystem;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class SwitchInputActionMap : CharacterMonitor
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


        public void SwitchActionMapDisableCursor(string newMap)
        {
            playerInput.SwitchCurrentActionMap(newMap);
            opsiveUnityInput.DisableCursor = true;          

        }

        public void SwitchActionMapEnableCursor(string newMap)
        {
            playerInput.SwitchCurrentActionMap(newMap);
            opsiveUnityInput.DisableCursor = false;
        }
    }
}