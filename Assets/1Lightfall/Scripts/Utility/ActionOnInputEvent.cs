using Opsive.Shared.Input;
using Opsive.Shared.Integrations.InputSystem;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MBS.Lightfall
{
    public class ActionOnInputEvent : CharacterMonitor
    {
        protected UnityEngine.InputSystem.PlayerInput playerInput;
        protected UnityInputSystem opsiveUnityInput;

        public string InputActionName;
        public UnityEvent Actions;

        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);

            if (character != null)
            {
                PlayerInputProxy inputProxy = character.GetComponentInChildren<PlayerInputProxy>();
                playerInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                opsiveUnityInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityInputSystem>();

                InputAction action = playerInput.actions.FindAction(InputActionName);
                if (action != null)
                    action.performed += PlayerInput_onActionTriggered;



            }
        }

        private void PlayerInput_onActionTriggered(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            if (obj.action.name == InputActionName)
                Actions.Invoke();
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                InputAction action = playerInput.actions.FindAction(InputActionName);
                if (action != null)
                    action.performed -= PlayerInput_onActionTriggered;
            }
        }
        private void OnEnable()
        {
            if (playerInput != null)
            {
                InputAction action = playerInput.actions.FindAction(InputActionName);
                if (action != null)
                    action.performed += PlayerInput_onActionTriggered;
            }
        }
    }
}