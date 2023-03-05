using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class ChangeStatsAbilityEffect : AbilityEffectBase
    {
        [SerializeField, Tooltip("Set to true if you want this effect to be toggled On-Off. Otherwise, set the duration.")]
        private bool onOffAbilityEffect = false;
        [SerializeField]
        private float duration;
        [SerializeField]
        private List<AbilityStatChangeEntry> statsToChange = new List<AbilityStatChangeEntry>();
        private List<AbilityStatChangeEntry> statsChanged;// used for undoing stat changes
        private ModifierHandler modifierHandler;

        private float remainingDuration;

        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);
            modifierHandler = abilityWrapper.Origin.GetComponent<ModifierHandler>();
            statsChanged = new List<AbilityStatChangeEntry>();
        }




        public override void Use(AbilityWrapperBase abilityWrapper)
        {

            if (abilityWrapper.AbilityState == AbilityState.Deactivating && onOffAbilityEffect)
            {
                OnEffectFinishedInvoke();
                return;
            }

            Dispose(abilityWrapper);


            float realVal;
            foreach (var item in statsToChange)
            {
                //get stat change value modified by any upgrades...
                realVal = abilityWrapper.GetStatChange(item.StatName, item.Value, false, true, false);

                //switch (item.StatName)
                //{
                //add special cases if necessary
                //example:
                //case StatName.DamageReduction:
                //    realVal = AbilityModifierFormulas.HardValueModifier(item.Value / 100, abilityWrapper.GetStatModifierValue(StatName.DamageReduction));
                //    break;

                //}

                //then apply the stat change to the modifierHandler
                modifierHandler.ChangeStatModifierValue(item.StatName, realVal);
                statsChanged.Add(new AbilityStatChangeEntry(item.StatName, realVal * 100));
            }

            if (!onOffAbilityEffect)
                remainingDuration = abilityWrapper.GetStatChange(StatName.AbilityDuration, duration, true);
            else
                OnEffectFinishedInvoke();
        }


        public override void OnUpdate(AbilityWrapperBase abilityWrapper)
        {
            base.OnUpdate(abilityWrapper);

            if (abilityWrapper.AbilityState == AbilityState.InUse && !onOffAbilityEffect)
            {
                if (remainingDuration > 0)
                    remainingDuration -= Time.deltaTime;
                else
                {
                    Dispose(abilityWrapper);
                    OnEffectFinishedInvoke();
                }

            }
        }


        public override void Dispose(AbilityWrapperBase abilityWrapperBase)
        {

            foreach (var item in statsChanged)
            {
                modifierHandler.ChangeStatModifierValue(item.StatName, item.Value * -1);
            }

            statsChanged.Clear();
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            foreach (var stat in statsToChange)
            {
                string displayName = "";
                //switch (stat.StatName)//add special cases for changing display names //Currently not needed since this applies changes non-locally
                //{
                //    case StatName.AbilityCapacity: displayName = "Capacity"; break;
                //    case StatName.AbilityRecharge: displayName = "Recharge Speed"; break;
                //    case StatName.AbilityDamage: displayName = "Damage"; break;
                //}

                string suffix = stat.flatValueChange ? "" : "%";
                int multiplier = stat.flatValueChange ? 1 : 100;
                returnVal.Add(new AbilityUIStat()
                {
                    StatName = stat.StatName,
                    StatNameDisplayName = displayName,
                    statValueDisplaySuffix = suffix,
                    InitalValue = stat.Value * multiplier,
                    CurrentValue = stat.Value * multiplier,
                    MaxValue = stat.Value * multiplier,
                    ProspectiveValue = stat.Value * multiplier
                });
            }

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityDuration,
                StatNameDisplayName = "Duration",
                statValueDisplaySuffix = " Sec",
                InitalValue = duration,
                CurrentValue = duration,
                MaxValue = duration,
                ProspectiveValue = duration
            });

            return returnVal;
        }


    }

    [Serializable]
    public class AbilityStatChangeEntry
    {
        public StatName StatName;
        [SerializeField, Tooltip("Percent change. actual value is divided by 100 to get standard floating point percent.")]
        private float value;
        public float Value { get => GetValue(); protected set => this.value = value; }
        [Tooltip("value can be interperted as a flat value increase instead of a percent. In these cases, the value is not divided by 100")]
        public bool flatValueChange;
        [SerializeField, Tooltip("For raw value increases, disable this. For percent increases, leave this enabled"), ShowIf("$flatValueChange")]
        private bool divideFlatValueBy100 = true;
        public bool DivideFlatValueBy100 { get => divideFlatValueBy100; }

        public AbilityStatChangeEntry(StatName statName, float value)
        {
            StatName = statName;
            this.value = value;
        }

        protected float GetValue()
        {
            float baseValue = value;
            if (StatName == StatName.AbilityRecharge)
                baseValue = value * -1f;

            float returnVal = (flatValueChange) ? baseValue : baseValue / 100;
            if (divideFlatValueBy100 && flatValueChange)
                returnVal /= 100;

            return returnVal;
        }
    }
}
