using MBS.StatsAndTags;
using System;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class DynamicStatModifier
    {
        public StatName Name { get; private set; }
        public float Value { get => GetValue(); }
        public event Action<float> OnValueChanged = delegate { };

        private float coreValue;//modifier value for high level game state, such as difficulty.
        private float externalValue; //modifier for dynamic values, such as temporary buffs or debuffs. Formula is core*external.


        public DynamicStatModifier(StatName name)
        {
            Name = name;
            coreValue = 1;// name.Default(); //Use this if you want the defaults for some stats to be different. Generally a multiplier of x1 for all seems fine atm.
            externalValue = 1;
        }

        /// <summary>
        /// This adds or subtracts from the current modifier and fires the OnValueChanged event. This does NOT "Set" the modifier value!
        /// </summary>
        /// <param name="modifier"></param>
        public void ChangeValue(float modifier, StatModifierType modType)
        {
            switch (modType)
            {
                case StatModifierType.Core: coreValue += modifier; break;
                case StatModifierType.External: externalValue += modifier; break;
            }


            OnValueChanged(Value);
        }

        private float GetValue()
        {
            //add other special cases here.
            float externalReturnVal = externalValue;
            switch (Name)
            {
                case StatName.DamageReduction:
                    if (externalReturnVal > 1.95f) externalReturnVal = 1.95f;
                    break;
            }

            //Clamped in order to prevent negative modifiers and divide by 0 errors (We don't want -.05% damage to heal 5% or something)
            float returnValue = Mathf.Clamp(coreValue * externalReturnVal, .001f, int.MaxValue);

            switch (Name)
            {
                case StatName.DamageReduction:
                    if (returnValue > 1.95f) returnValue = 1.95f;
                    break;
            }

            return returnValue;
        }



    }
}
