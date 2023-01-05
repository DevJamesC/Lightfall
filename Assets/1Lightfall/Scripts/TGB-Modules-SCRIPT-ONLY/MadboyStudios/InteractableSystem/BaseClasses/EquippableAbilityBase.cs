using MBS.AbilitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.InteractionSystem
{
    public class EquippableAbilityBase : EquippableBase
    {
        protected AbilityWrapperBase sourceAbilityWrapper;
        /// <summary>
        /// Used to indicate if an ability-equipment has been used at all. If it has been somewhat or fully used, this will be true.
        /// </summary>
        public bool PartiallyUsed { get; protected set; }

        public virtual void Equip(AbilityWrapperBase abilityWrapper)
        {
            sourceAbilityWrapper = abilityWrapper;
            PartiallyUsed = false;
        }

        public override void Use()
        {
            PartiallyUsed = true;
            base.Use();
        }

        public virtual List<AbilityUIStat> GetStats()
        {
            Debug.LogWarning($"You are trying to display stats for equippable ability {GetType()}, but it has no override for GetStats.");
            return new List<AbilityUIStat>();
        }
    }
}
