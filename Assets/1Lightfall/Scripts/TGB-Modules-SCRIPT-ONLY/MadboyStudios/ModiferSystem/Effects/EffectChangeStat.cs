using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class EffectChangeStat : ModifierEffectBase
    {

        [SerializeField]
        private StatName statName;
        [SerializeField]
        private float percentChange;
        [SerializeField]
        private StatModifierType changeType = StatModifierType.External;
        [SerializeField, ReadOnly]
        private string ValueAsMultiplier;

        private float realPercent { get => percentChange / 100; }

        public override void EffectActivated(ModifierEntry target)
        {
            base.EffectActivated(target);
            target.Target.ChangeStatModifierValue(statName, realPercent, changeType);
        }

        public override void EffectRemoved(ModifierEntry target)
        {
            base.EffectRemoved(target);
            target.Target.ChangeStatModifierValue(statName, -realPercent, changeType);
        }

        public override void EffectUpdate(ModifierEntry target)
        {
            base.EffectUpdate(target);
        }

        public override void OnValidate()
        {
            base.OnValidate();
            ValueAsMultiplier = "1 + " + realPercent.ToString() + " = " + (1 + realPercent).ToString() + " " + statName.ToString();
        }

        public override string ToString()
        {
            string returnVal;
            string hasPlus = realPercent > 0 ? "+" : "";

            returnVal = $"Stat {statName}: {hasPlus}{realPercent}";

            return returnVal;
        }
        //when adding GetStats, the suffix should be "%"
    }

    public enum StatModifierType
    {
        Core, //Refers to stat changes based on overall game state, such as difficulty
        External //Refers to stat changes based on dynamic abilites during a play session
    }
}
