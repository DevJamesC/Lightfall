using MBS.InteractionSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class EquipOnUseAbilityEffect : AbilityEffectBase
    {
        [SerializeField]
        private EquippableAbilityBase abilitySpawnableEquipment;
        private HandleEquipmentDemoForAbilityEquipment equipmentHandler;

        private EquippableAbilityBase currentlyEquipped;

        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            //Make HasRecharge or HasCharge so it only activates rechage or charge use if the ability is used and not canceled.
            base.OnStart(abilityWrapper);
            equipmentHandler = abilityWrapper.Origin.gameObject.GetComponent<HandleEquipmentDemoForAbilityEquipment>();
            abilityWrapper.CancelableOnOtherAbilityCast = true;
        }

        public override void Use(AbilityWrapperBase abilityWrapper)
        {
            //equip ability and pass in any local stats in abilityWrapper from upgrades 
            //pass in delegates for OnUse on the ability-equipment (so when the equipment is used, it calls this's OnAbilityFinishedInvoke() )
            currentlyEquipped = equipmentHandler.Equip(abilitySpawnableEquipment) as EquippableAbilityBase; //equipped in equipmentHandler
            currentlyEquipped.Equip(abilityWrapper); //Firing the EquippableAbilityBase.Equip() to fire any initalization code for the object.
            currentlyEquipped.OnFinishUse += () => { UnEquipAbility(); OnEffectFinishedInvoke(); };
        }

        public override void UseWhileInUse(AbilityWrapperBase abilityWrapper)
        {
            //If nothing is equipped, then something has gone wrong and we should cancel.
            if (currentlyEquipped == null)
            {
                UnEquipAbility();
                abilityWrapper.CancelAbility(false);
                return;
            }

            bool putAwayBeforeAnyUse = !currentlyEquipped.PartiallyUsed;
            //Unequip the ability
            UnEquipAbility();

            //If the ability has not been used, cancel the ability. Otherwise, finish the ability as if it has been fully used.
            if (putAwayBeforeAnyUse)
                abilityWrapper.CancelAbility(false);
            else
                OnEffectFinishedInvoke();
        }

        public override void Dispose(AbilityWrapperBase abilityWrapperBase)
        {
            base.Dispose(abilityWrapperBase);

            if (currentlyEquipped != null)
                UnEquipAbility();
        }

        public override List<AbilityUIStat> GetStats()
        {

            return abilitySpawnableEquipment.GetStats();
        }

        private void UnEquipAbility()
        {
            //stow ability
            equipmentHandler.UnEquip();

            currentlyEquipped = null;
        }


    }
}