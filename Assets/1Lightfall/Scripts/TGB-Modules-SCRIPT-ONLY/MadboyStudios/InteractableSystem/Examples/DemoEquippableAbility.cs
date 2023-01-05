using MBS.AbilitySystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.InteractionSystem
{
    public class DemoEquippableAbility : EquippableAbilityBase
    {
        [SerializeField]
        private float damage;
        [SerializeField]
        private float radius;
        [SerializeField, ReadOnly]
        private int charges = 0;

        HandleEquipmentDemoForAbilityEquipmentInput input;

        public override void Equip(AbilityWrapperBase abilityWrapper)
        {
            base.Equip(abilityWrapper);

            input = abilityWrapper.Origin.GetComponent<HandleEquipmentDemoForAbilityEquipmentInput>();
            charges = sourceAbilityWrapper.ChargesRemaining;

            //Ideally, input delegation would not happen on the ability itself, but rather in a "playerInput" monobehavior or something (like a playerAbilityInputHandler, or just in the abilityLoadout)
            input.MainKeyDown += Input_MainKeyDown;
            input.AlternateKeyDown += Input_AlternateKeyDown;
            input.Alternate2KeyDown += Input_Alternate2KeyDown;
        }

        public override void Unequip(HandleEquipmentDemoForAbilityEquipment handler)
        {
            base.Unequip(handler);
            Destroy(gameObject);
        }

        private void Input_MainKeyDown()
        {
            Debug.Log($"Demo Ability Main use. Damage: {sourceAbilityWrapper.GetStatChange(StatName.AbilityDamage, damage, true)}  Radius: {sourceAbilityWrapper.GetStatChange(StatName.AbilityRadius, radius, true)}");
            Use();
        }
        private void Input_AlternateKeyDown()
        {
            Debug.Log($"Demo Ability Alternate use. Damage: {sourceAbilityWrapper.GetStatChange(StatName.AbilityDamage, damage, true)}  Radius: {sourceAbilityWrapper.GetStatChange(StatName.AbilityRadius, radius, true)}");
            Use();
        }
        private void Input_Alternate2KeyDown()
        {
            Debug.Log($"Demo Ability Alternate2 use. Damage: {sourceAbilityWrapper.GetStatChange(StatName.AbilityDamage, damage, true)}  Radius: {sourceAbilityWrapper.GetStatChange(StatName.AbilityRadius, radius, true)}");
            Use();
        }

        public override void Use()
        {
            base.Use();
            charges--;
            sourceAbilityWrapper.ChangeChargesRemaining(-1);
            //Check if the equipment is "finished"
            if (charges <= 0)
                OnFinishUseInvoke();
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityDamage,
                StatNameDisplayName = "Damage",
                InitalValue = damage,
                CurrentValue = damage,
                MaxValue = damage,
                ProspectiveValue = damage
            });

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityRadius,
                StatNameDisplayName = "Radius",
                InitalValue = radius,
                CurrentValue = radius,
                MaxValue = radius,
                ProspectiveValue = radius
            });

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityCapacity,
                StatNameDisplayName = "Charges",
                InitalValue = charges,
                CurrentValue = charges,
                MaxValue = charges,
                ProspectiveValue = charges
            });

            return returnVal;
        }



        private void OnDisable()
        {
            input.MainKeyDown -= Input_MainKeyDown;
            input.AlternateKeyDown -= Input_AlternateKeyDown;
            input.Alternate2KeyDown -= Input_Alternate2KeyDown;
        }
    }
}
