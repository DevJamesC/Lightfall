using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using MBS.Lightfall;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Modules;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class EquipOpsiveItemOnUse : AbilityEffectBase
    {
        [Tooltip("The Item that will be equipped when this ability is used.")]
        [SerializeField] private ItemType itemSO;
        [Tooltip("Does the item consume a charge every use, or does it have its own ammo pool which consumes a charge when completely expended (or if the ability is canceld after partially used)")]
        [SerializeField] private bool UseChargeEveryUse;

        private Inventory inventory;
        private ItemSetManager itemSetManager;
        private EquipUnequip characterAbilityEquipUnequip;
        private CharacterItem equippedItem;
        private int slotID;
        private static Dictionary<GameObject, LastEquippedData> previouslyEquippedItemIndex;

        private AbilityItemHandler abilityItemHandler;
        private bool waitingForEquipToComplete;
        private bool unequipping;

        private Action removeItemsAction;
        private ThrowableAction throwableAction;
        [Tooltip("Fill out the base stats for the item.")]
        public List<AbilityUIStat> ItemBaseStats = new List<AbilityUIStat>();
        //TODO: need to impliment a "gain charges" method and pattern

        public event Action<AbilityItemHandler> OnSetupAbilityItemHandler = delegate { };

        public override bool CanBeCanceled
        {
            get
            {
                if (waitingForEquipToComplete || unequipping)
                    return false;

                if (abilityItemHandler != null)
                    return !abilityItemHandler.IsInUse;

                return true;
            }
        }

        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);
            if (previouslyEquippedItemIndex == null)
                previouslyEquippedItemIndex = new Dictionary<GameObject, LastEquippedData>();
            if (!previouslyEquippedItemIndex.ContainsKey(abilityWrapper.gameObject))
                previouslyEquippedItemIndex.Add(abilityWrapper.gameObject, new LastEquippedData());
            inventory = abilityWrapper.Origin.gameObject.GetComponent<Inventory>();
            itemSetManager = abilityWrapper.Origin.gameObject.GetComponent<ItemSetManager>();
            UltimateCharacterLocomotion locomotionComp = abilityWrapper.Origin.gameObject.GetComponent<UltimateCharacterLocomotion>();
            characterAbilityEquipUnequip = locomotionComp.GetAbility<EquipUnequip>();
            abilityWrapper.CancelableOnOtherAbilityCast = true;
            slotID = itemSO.Prefabs[0].GetComponent<CharacterItem>().SlotID;
            previouslyEquippedItemIndex[abilityWrapper.gameObject].index = 0;
            waitingForEquipToComplete = false;
            unequipping = false;
            throwableAction = null;



            abilityWrapper.OnCanceled += UnEquipAbility;
            //abilityWrapper.OnFinishUse += UnEquipAbility;

            inventory.OnEquipItemEvent.AddListener((item, integer) => { OnEquipComplete(abilityWrapper); });
            inventory.OnUnequipItemEvent.AddListener((item, integer) => { CheckIfSwappedAwayFromAbility(item, abilityWrapper); });
            removeItemsAction += () => { if (equippedItem != null) equippedItem.Drop(10000, true, true); equippedItem = null; unequipping = false; };

        }

        public override void Use(AbilityWrapperBase abilityWrapper)
        {
            if (waitingForEquipToComplete)
                return;

            if (unequipping && equippedItem != null)
                return;


            if (!previouslyEquippedItemIndex[abilityWrapper.gameObject].abilityInUse)
                previouslyEquippedItemIndex[abilityWrapper.gameObject].index = characterAbilityEquipUnequip.ActiveItemSetIndex;

            previouslyEquippedItemIndex[abilityWrapper.gameObject].abilityInUse = true;
            inventory.AddItemIdentifierAmount(itemSO, 1);
            //instant swap away from current item
            CharacterItem currentlyEquippedItem = inventory.GetActiveCharacterItem(slotID);
            float oldDuration = 0;
            bool oldAnimationEventOption = false;
            if (currentlyEquippedItem != null)
            {
                oldDuration = currentlyEquippedItem.EquipCompleteEvent.Duration;
                oldAnimationEventOption = currentlyEquippedItem.EquipCompleteEvent.WaitForAnimationEvent;
                currentlyEquippedItem.UnequipEvent.Duration = 0;
                currentlyEquippedItem.UnequipEvent.WaitForAnimationEvent = false;
            }
            characterAbilityEquipUnequip.StartEquipUnequip(itemSetManager.ItemSetGroups[0].ItemSetList.Count - 1, true);
            if (currentlyEquippedItem != null)
            {
                currentlyEquippedItem.UnequipEvent.Duration = oldDuration;
                currentlyEquippedItem.UnequipEvent.WaitForAnimationEvent = oldAnimationEventOption;
            }
            waitingForEquipToComplete = true;
        }
        /// <summary>
        /// This handles setting up the newly equipped item with AbilityItemHandler to handle MBS ability related stuff. 
        /// If StartEquipUnequip in Use() was instant, then this method could be in place of "waitingForEquipToComplete = true".
        /// </summary>
        /// <param name="abilityWrapper"></param>
        private void OnEquipComplete(AbilityWrapperBase abilityWrapper)
        {
            if (!waitingForEquipToComplete)
                return;

            unequipping = false;
            waitingForEquipToComplete = false;
            equippedItem = inventory.GetActiveCharacterItem(slotID);
            ResetModules(equippedItem);

            abilityItemHandler = equippedItem.gameObject.GetComponent<AbilityItemHandler>();
            if (abilityItemHandler == null)
                abilityItemHandler = equippedItem.gameObject.AddComponent<AbilityItemHandler>();

            abilityItemHandler.Initalize(abilityWrapper);
            abilityItemHandler.OnFinishUse += () =>
            {
                if (equippedItem != null)
                {
                    UnEquipAbility(abilityWrapper);
                    OnEffectFinishedInvoke();
                    if (!UseChargeEveryUse && abilityItemHandler.PercentUseRemaining < 1)
                    {
                        abilityWrapper.ChangeChargesRemaining(-1);
                    }
                }
            };
            if (UseChargeEveryUse)
                abilityItemHandler.OnUse += () => { abilityWrapper.ChangeChargesRemaining(-1); };

            OnSetupAbilityItemHandler(abilityItemHandler);
        }

        //some modules need to be manually reset sometimes, for some reason. If things don't equip right, likely more use cases need to be applied here.
        private void ResetModules(CharacterItem characterItem)
        {
            List<TriggerModule> triggerMods = characterItem.GetComponent<UsableAction>().TriggerActionModuleGroup.Modules.ToList();
            foreach (var mod in triggerMods)
            {
                mod.ResetModule(false);
            }
            //If a throwable... (may need to check if shootable too...)
            if (throwableAction == null)
                throwableAction = characterItem.GetComponent<ThrowableAction>();
            if (throwableAction != null)
            {
                List<ThrowableProjectileModule> throwableMods = throwableAction.ProjectileModuleGroup.Modules.ToList();
                foreach (var mod in throwableMods)
                {
                    SpawnProjectile spawnProjectile = mod as SpawnProjectile;
                    if (spawnProjectile != null)
                        spawnProjectile.EnableObjectMeshRenderers(true);

                }
            }
        }

        private void CheckIfSwappedAwayFromAbility(CharacterItem item, AbilityWrapperBase abilityWrapper)
        {
            if (unequipping)
                return;

            if (equippedItem == null)
                return;

            if (equippedItem == item)
                UseWhileInUse(abilityWrapper);


        }

        public override void UseWhileInUse(AbilityWrapperBase abilityWrapper)
        {
            if (waitingForEquipToComplete)
                return;

            if (unequipping && equippedItem != null)
                return;

            if (abilityItemHandler == null)
            {
                abilityWrapper.CancelAbility(false);
                return;
            }

            if (abilityItemHandler.IsInUse)
            {
                return;
            }

            bool putAwayBeforeAnyUse = abilityItemHandler.PercentUseRemaining >= 1;
            //If the ability has not been used, cancel the ability. Otherwise, finish the ability as if it has been fully used.
            if (putAwayBeforeAnyUse)
            {
                abilityItemHandler.OnFinishUse = delegate { };
                abilityItemHandler.OnUse = delegate { };
                abilityWrapper.CancelAbility(false);
            }
            else
            {
                abilityItemHandler.OnFinishUse = delegate { };
                abilityItemHandler.OnUse = delegate { };
                abilityWrapper.CancelAbility(true);
            }
        }

        public override void Dispose(AbilityWrapperBase abilityWrapperBase)
        {
            base.Dispose(abilityWrapperBase);
            UnEquipAbility(abilityWrapperBase);
        }

        public override List<AbilityUIStat> GetStats()
        {

            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            foreach (var item in ItemBaseStats)
            {
                returnVal.Add(item.Copy());
            }

            return returnVal;
        }

        private void UnEquipAbility(AbilityWrapperBase abilityWrapperBase)
        {
            if (unequipping)
                return;

            unequipping = true;
            waitingForEquipToComplete = false;

            if (equippedItem != null)
            {
                characterAbilityEquipUnequip.StartEquipUnequip(previouslyEquippedItemIndex[abilityWrapperBase.gameObject].index, true);
                previouslyEquippedItemIndex[abilityWrapperBase.gameObject].abilityInUse = false;
                Scheduler.Schedule(equippedItem.UnequipEvent.Duration + .5f, removeItemsAction);

            }

            abilityItemHandler = null;
            previouslyEquippedItemIndex[abilityWrapperBase.gameObject].index = 0;


        }

        private class LastEquippedData
        {
            public int index;
            public bool abilityInUse;
        }
    }

}
