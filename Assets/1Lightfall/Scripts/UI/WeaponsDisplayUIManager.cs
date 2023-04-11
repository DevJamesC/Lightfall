using Michsky.MUIP;
using Opsive.Shared.Events;
using Opsive.Shared.Input;
using Opsive.Shared.Integrations.InputSystem;
using Opsive.Shared.Inventory;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MBS.Lightfall
{
    public class WeaponsDisplayUIManager : CharacterMonitor
    {
        protected UnityEngine.InputSystem.PlayerInput playerInput;
        protected UnityInputSystem opsiveUnityInput;
        protected ItemSetManager itemSetManager;
        protected LightfallEquipmentLoadout loadoutScript;

        [SerializeField] protected ButtonManager primaryEquipmentBtn;
        [SerializeField] protected ButtonManager secondaryEquipmentBtn;
        [SerializeField] protected ItemDetailDisplayUI itemDetailsRootGameobject;


        private EquipmentSelectState selectionState;
        private int selectionIndex;
        private ItemDetailsScriptableObject currentlyViewedItem;

        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);

            if (character != null)
            {
                PlayerInputProxy inputProxy = character.GetComponentInChildren<PlayerInputProxy>();
                playerInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                opsiveUnityInput = inputProxy.PlayerInput.gameObject.GetComponent<UnityInputSystem>();
                itemSetManager = character.GetComponent<ItemSetManager>();
                loadoutScript = character.GetComponent<LightfallEquipmentLoadout>();

            }
        }

        protected override void Start()
        {
            base.Start();
            selectionState = EquipmentSelectState.None;
            selectionIndex = 0;
            itemDetailsRootGameobject.gameObject.SetActive(false);
            primaryEquipmentBtn.onClick.AddListener(() => SetReceivingLoadoutSlot(EquipmentSelectState.Primary));
            secondaryEquipmentBtn.onClick.AddListener(() => SetReceivingLoadoutSlot(EquipmentSelectState.Secondary));
            EventHandler.RegisterEvent("OpenEquipmentLoadoutUI", OnOpen);
            EventHandler.RegisterEvent("CloseEquipmentLoadoutUI", OnClose);
            GetComponentInChildren<WindowManager>(true).onWindowChange.AddListener(OnWindowChange);
        }

        private void OnOpen()
        {
            if (loadoutScript == null)
                return;

            UpdateEquipmentButtonValue();
        }

        private void UpdateEquipmentButtonValue()
        {
            primaryEquipmentBtn.buttonText = loadoutScript.PrimaryEquipment != null ? loadoutScript.PrimaryEquipment.ItemName : "Empty";
            secondaryEquipmentBtn.buttonText = loadoutScript.SecondaryEquipment != null ? loadoutScript.SecondaryEquipment.ItemName : "Empty";
            primaryEquipmentBtn.UpdateUI();
            secondaryEquipmentBtn.UpdateUI();
        }

        private void OnClose()
        {
            SetReceivingLoadoutSlot(EquipmentSelectState.None);
        }

        private void OnWindowChange(int newWindow)
        {
            SetReceivingLoadoutSlot(EquipmentSelectState.None);
        }

        private void SetReceivingLoadoutSlot(EquipmentSelectState selectionState)
        {
            if (this.selectionState != EquipmentSelectState.None)
                DeinitalizeWeaponDetails();

            this.selectionState = selectionState;

            switch (this.selectionState)
            {
                case EquipmentSelectState.Primary: InitalizeWeaponDetails(); break;
                case EquipmentSelectState.Secondary: InitalizeWeaponDetails(); break;
                case EquipmentSelectState.None: DeinitalizeWeaponDetails(); break;
            }
        }

        private void InitalizeWeaponDetails()
        {
            selectionIndex = 0;
            List<ItemDetailsScriptableObject> items = OwnedItemsManager.Instance.GetItemsByCategory(itemSetManager.ItemSetGroups[0].ItemCategory, true);
            ItemDetailsScriptableObject currentItem = selectionState == EquipmentSelectState.Primary ? loadoutScript.PrimaryEquipment : loadoutScript.SecondaryEquipment;
            int startingIndex = 0;
            foreach (ItemDetailsScriptableObject item in items)
            {
                if (item == currentItem)
                {
                    selectionIndex = startingIndex;
                    break;
                }
                startingIndex++;
            }

            if (!UpdateItemDetail())
                return;

            itemDetailsRootGameobject.OnNextPressed += ItemDetailsRootGameobject_OnNextPressed;
            itemDetailsRootGameobject.OnPreviousPressed += ItemDetailsRootGameobject_OnPreviousPressed;
            itemDetailsRootGameobject.OnSelectPressed += ItemDetailsRootGameobject_OnSelectPressed;
            itemDetailsRootGameobject.OnCancelPressed += ItemDetailsRootGameobject_OnCancelPressed;


            itemDetailsRootGameobject.gameObject.SetActive(true);
        }

        private void DeinitalizeWeaponDetails()
        {
            itemDetailsRootGameobject.gameObject.SetActive(false);
            currentlyViewedItem = null;
            selectionIndex = 0;
            itemDetailsRootGameobject.OnNextPressed -= ItemDetailsRootGameobject_OnNextPressed;
            itemDetailsRootGameobject.OnPreviousPressed -= ItemDetailsRootGameobject_OnPreviousPressed;
            itemDetailsRootGameobject.OnSelectPressed -= ItemDetailsRootGameobject_OnSelectPressed;
            itemDetailsRootGameobject.OnCancelPressed -= ItemDetailsRootGameobject_OnCancelPressed;
        }

        private bool UpdateItemDetail()
        {
            List<ItemDetailsScriptableObject> items = OwnedItemsManager.Instance.GetItemsByCategory(itemSetManager.ItemSetGroups[0].ItemCategory, true);
            if (items.Count <= selectionIndex)
                selectionIndex = items.Count - 1;
            if (selectionIndex < 0)
            {
                DeinitalizeWeaponDetails();
                Debug.LogWarning("selection index was -1. Cancelling weapon select display.");
                return false;
            }
            currentlyViewedItem = items[selectionIndex];
            itemDetailsRootGameobject.InitalizeDetails(currentlyViewedItem, selectionIndex < items.Count - 1, selectionIndex != 0);
            itemDetailsRootGameobject.UpdateItemCountText($"{selectionIndex}/{items.Count - 1}", true);
            return true;
        }

        private void ItemDetailsRootGameobject_OnPreviousPressed()
        {
            selectionIndex -= 1;
            if (selectionIndex < 0)
                selectionIndex = 0;
            UpdateItemDetail();
        }

        private void ItemDetailsRootGameobject_OnNextPressed()
        {
            selectionIndex += 1;
            UpdateItemDetail();
        }

        private void ItemDetailsRootGameobject_OnCancelPressed()
        {
            SetReceivingLoadoutSlot(EquipmentSelectState.None);
        }

        private void ItemDetailsRootGameobject_OnSelectPressed()
        {
            LightfallEquipmentLoadout.LoadoutEquipmentType slotType = LightfallEquipmentLoadout.LoadoutEquipmentType.Primary;
            //figure out if this is a primary or secondary equip, and set the button text to the item name
            if (selectionState == EquipmentSelectState.Primary)
            {
                slotType = LightfallEquipmentLoadout.LoadoutEquipmentType.Primary;
                primaryEquipmentBtn.buttonText = currentlyViewedItem.ItemName;
            }
            if (selectionState == EquipmentSelectState.Secondary)
            {
                slotType = LightfallEquipmentLoadout.LoadoutEquipmentType.Secondary;
                secondaryEquipmentBtn.buttonText = currentlyViewedItem.ItemName;
            }

            //equip new item
            loadoutScript.ChangeLoadoutEquipment(currentlyViewedItem, slotType);
            UpdateEquipmentButtonValue();

            DeinitalizeWeaponDetails();
        }



        private enum EquipmentSelectState
        {
            None,
            Primary,
            Secondary
        }



        //Get weapons list
        //Display weapons
        //Select weapon and equip it to character via the loadout script

        //get animations for the display (open, close, show next, show previous)
        //Get loadout UI to be seperate scene?
    }
}
