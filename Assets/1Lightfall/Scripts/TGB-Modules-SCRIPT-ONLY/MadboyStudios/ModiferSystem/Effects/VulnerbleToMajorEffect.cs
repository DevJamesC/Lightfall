using MBS.AbilitySystem;
using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    /// <summary>
    /// This class just acts as a tag for EffectsWithIntensityBase.
    /// Vulnerble increases the intensity for the next ability that hits it, then it is consumed by the effectWithIntensity
    /// </summary>
    public class VulnerbleToMajorEffect : ModifierEffectBase
    {
        [SerializeField]
        private MajorEffects vulnerbleTo;
        public MajorEffects VulnerbleTo { get => vulnerbleTo; }

        public DetonationsSO detonationsSO;

        public override void EffectActivated(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);
            List<ModifierEntry> entries = targetEntry.Target.GetModifierEntries<EffectWithIntensityBase>();
            foreach (var entry in entries)
            {
                EffectWithIntensityBase effect = entry.Effect as EffectWithIntensityBase;
                if (effect == null)
                    continue;

                EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;
                int detonationLevel = data.Intensity + 1;
                //cause a detonation instead, if the target is already primed for this vulnerbility type
                if (effect.EffectCategory == vulnerbleTo)
                {
                    detonationsSO.Detonate(effect, data, vulnerbleTo, entry.Target, targetEntry.Origin.gameObject, detonationLevel);
                    targetEntry.RemainingDuration = -1;
                    entry.RemainingDuration = -1;
                    return;
                }


            }
        }

        public override List<AbilityUIStat> GetStats()
        {
            //should retun an icon
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat
            {
                StatName = StatName.None,
                EffectIcon = UIDisplayIcon
            });

            return returnVal;
        }

        public override void ApplyAbilitySystemUpgradesToEntries(ModifierEntry entryForEffect, AbilityWrapperBase abilityWrapper)
        {

        }
    }
}
