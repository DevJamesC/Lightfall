using MBS.DamageSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class Shield : ValuePool
    {
        [SerializeField, Range(0, 1), Tooltip("The amount of overbleed allowed. 1 is all overbleed damage goes into health. 0 is all overbleed damage is lost.")]
        private float OverbleedMultiplier = 1;

        private ModifierHandler modifierHandler;

        private void Awake()
        {
            modifierHandler = GetComponent<ModifierHandler>();
        }
        public override void TakeDamage(DamageData damageData, Collider collider = null)
        {
            if (!IsAlive || !enabled)
                return;

            InvokePreDamageEvent(damageData);

            int damageToShields = Mathf.RoundToInt(damageData.Damage * damageData.SheildEffectiveness);
            int overflow = Mathf.RoundToInt(((damageToShields - currentValue) / damageData.SheildEffectiveness) * OverbleedMultiplier);
            DecreaseCurrentValue(damageData.Damage);

            InvokePostDamageEvent(damageData);

            damageData.SetDamage(overflow);
        }

        protected override void ResetValuePoolComponent()
        {
            currentValue = maxValue * modifierHandler.GetStatModifierValue(StatName.MaxShield);
        }


    }
}
