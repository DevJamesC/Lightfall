using MBS.Lightfall;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.Shared.Integrations.InputSystem;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MBS.AbilitySystem
{
    public class DisplayAbilityListMenu : CharacterMonitor
    {

        public AbilityOverviewButton abilityOverviewButtonPrefab;

        protected GameObject ObjectWithAbilitiesToView;
        protected UnityEngine.InputSystem.PlayerInput playerInput;
        protected UnityInputSystem opsiveUnityInput;
        private List<AbilityAndUpgradePair> abilityUpgradePair;
        private List<AbilityOverviewButton> displayedAbilityElements;


        protected override void Awake()
        {
            base.Awake();
            abilityUpgradePair = new List<AbilityAndUpgradePair>();
            displayedAbilityElements = new List<AbilityOverviewButton>();
        }

        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);
            ObjectWithAbilitiesToView = character;
            if (character != null)
            {
                PlayerInputProxy inputProxy = character.GetComponentInChildren<PlayerInputProxy>();
                playerInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                opsiveUnityInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityInputSystem>();
                Scheduler.Schedule(1f, () => { EventHandler.ExecuteEvent(gameObject, "OnOpenCloseAbilityMenu", true); });
            }


        }

        private void OpenCloseAbilityMenu(bool open)
        {
            if (open)
            {
                GetAbilityData(ObjectWithAbilitiesToView);
                DrawAbilityList();
                playerInput.SwitchCurrentActionMap("UI");
                opsiveUnityInput.DisableCursor = false;
            }
            else
            {
                ClearUI();
                playerInput.SwitchCurrentActionMap("Gameplay");
                opsiveUnityInput.DisableCursor = true;
                Debug.Log("the 'enable-disable cursor and UI Input Mode' logic will need to be moved to a more centralized location.");
            }

        }

        private void GetAbilityData(GameObject sourceObject)
        {
            abilityUpgradePair.Clear();

            //AbilityLoadout loadout = sourceObject.GetComponent<AbilityLoadout>();
            //if (loadout == null)
            //{
            //    Debug.LogWarning($"Trying to display abilites for {sourceObject.name}, which does not contain an AbilityLoadout.");
            //    return;
            //}
            //foreach (var abilityUpPair in loadout.Abilities)
            //{
            //    abilityUpgradePair.Add(abilityUpPair);
            //}

            UltimateCharacterLocomotion characterLocomotion = sourceObject.GetComponent<UltimateCharacterLocomotion>();
            foreach (var ability in characterLocomotion.Abilities)
            {
                LightfallAbilityBase lightfallAbility = ability as LightfallAbilityBase;
                if (lightfallAbility != null)
                {
                    abilityUpgradePair.Add(new AbilityAndUpgradePair() { AbilitySO = lightfallAbility.abilitySO, Upgrades = lightfallAbility.UpgradeData });

                }
            }


        }
        private void DrawAbilityList()
        {
            //clear current abilities displaying if any exist
            ClearUI();

            //redraw abilities
            foreach (var abilityUpPair in abilityUpgradePair)
            {
                AbilityOverviewButton abilityOverviewButtonObj = Instantiate(abilityOverviewButtonPrefab, transform);
                string abilityTitle = abilityUpPair.AbilitySO.Title == "" ? abilityUpPair.AbilitySO.name : abilityUpPair.AbilitySO.Title;
                abilityOverviewButtonObj.Initalize(abilityUpPair, this, abilityTitle, " Level " + abilityUpPair.Upgrades.GetUIUpgradeLevel());
                abilityOverviewButtonObj.OnInitalizingAbilityDetail += () => { gameObject.SetActive(false); };

                displayedAbilityElements.Add(abilityOverviewButtonObj);
            }
        }

        private void ClearUI()
        {
            foreach (var item in displayedAbilityElements)
            {
                Destroy(item.gameObject);
            }
            displayedAbilityElements.Clear();
        }

        private void OnDisable()
        {
            //if (ObjectWithAbilitiesToView != null)
            //ObjectWithAbilitiesToView.GetComponent<AbilityLoadout>().AbilitiesChanged -= DrawAbilityList;

            EventHandler.UnregisterEvent<bool>(gameObject, "OnOpenCloseAbilityMenu", OpenCloseAbilityMenu);
        }



        private void OnEnable()
        {
            DrawAbilityList();
            //ObjectWithAbilitiesToView.GetComponent<AbilityLoadout>().AbilitiesChanged += DrawAbilityList;

            EventHandler.RegisterEvent<bool>(gameObject, "OnOpenCloseAbilityMenu", OpenCloseAbilityMenu);
        }


    }
}
