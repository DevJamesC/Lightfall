using MBS.DamageSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class DetonateMajorEffect : AbilityEffectBase
    {
        [SerializeField, Tooltip("The icon to represent a detonator effect in the ability UI")]
        private Sprite AbilityEffectStatUIIcon;
        [SerializeField]
        private DetonationsSO detonationsSO;
        [SerializeField]
        private MajorEffects detonatorType;

        int detonationLevel;

        GameObject sourceObject;
        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);

            switch (abilityWrapper.UpgradeData.GetUIUpgradeLevel())
            {
                case 0:
                case 1:
                case 2: detonationLevel = 1; break;
                case 3:
                case 4: detonationLevel = 2; break;
                case 5: detonationLevel = 3; break;
                default: detonationLevel = 1; break;
            }
            sourceObject = abilityWrapper.Origin;
            abilityWrapper.OnDealDamage += TryDetonate;
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.None,
                EffectIcon = AbilityEffectStatUIIcon
            });
            return returnVal;
        }

        private void TryDetonate(IDamageable obj, DamageData damageData)
        {
            //get modifier, check if it has anything to detonate, then detonate it
            ModifierHandler targetModifierHandler = obj.gameObject.GetComponent<ModifierHandler>();
            if (targetModifierHandler == null)
                return;

            List<ModifierEntry> entries = targetModifierHandler.GetModifierEntries<EffectWithIntensityBase>();

            //look for detonators of opposite type, use the highest radius and highest damage,
            //then use the highest intensity of an effect of an opposite type, or the highest intensity of all effects if none are opposite
            List<ModifierEntry> entriesOfOppositeType = new List<ModifierEntry>();
            float highestRadiusModifier = 1;
            int highestIntensityOfOpposite = 1;
            int highestIntensityOfAll = 1;

            List<MajorEffects> reactableEffects = detonationsSO.GetReactors(detonatorType);
            foreach (var entry in entries)
            {
                EffectWithIntensityBase effect = entry.Effect as EffectWithIntensityBase;
                if (effect == null)
                    continue;
                EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;

                if (data.DetonationSizeModifier > highestRadiusModifier)
                    highestRadiusModifier = data.DetonationSizeModifier;

                //check if the entry is of the opposite type
                if (reactableEffects.Contains(effect.EffectCategory))
                {
                    entriesOfOppositeType.Add(entry);
                    if (data.Intensity > highestIntensityOfOpposite)
                        highestIntensityOfOpposite = data.Intensity;
                }
                else
                {
                    if (effect.EffectCategory != MajorEffects.Balefiring && effect.EffectCategory != MajorEffects.Warping)
                    {
                        if (data.Intensity > highestIntensityOfAll)
                            highestIntensityOfAll = data.Intensity;

                        entry.RemainingDuration = -1;
                    }

                }
            }

            //if we have entries of the opposite type, figure out which one to detonate against
            if (entriesOfOppositeType.Count > 0)
            {
                ModifierEntry entryToDetonateAainst = entriesOfOppositeType[0];
                EffectWithIntensityBase effectToDetonateAgainst = entriesOfOppositeType[0].Effect as EffectWithIntensityBase;
                EffectWithIntensityData dataToDetonateAgainst = entriesOfOppositeType[0].EffectStateData as EffectWithIntensityData;
                foreach (var entry in entriesOfOppositeType)
                {
                    if (effectToDetonateAgainst.EffectCategory == reactableEffects[0])
                        continue;

                    EffectWithIntensityBase effect = entry.Effect as EffectWithIntensityBase;
                    if (effect == null)
                        continue;

                    EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;

                    if (effect.EffectCategory == reactableEffects[0])
                    {
                        entryToDetonateAainst = entry;
                        effectToDetonateAgainst = effect;
                        dataToDetonateAgainst = data;
                        break;
                    }

                }
                detonationsSO.Detonate(effectToDetonateAgainst, dataToDetonateAgainst, detonatorType, entryToDetonateAainst.Target, sourceObject, detonationLevel + highestIntensityOfOpposite);

            }//else just do a standard detonation of detonator type, detonating against the first effect in the list
            else if (entries.Count > 0)
            {
                foreach (var entry in entries)
                {

                    EffectWithIntensityBase effect = entry.Effect as EffectWithIntensityBase;
                    if (effect == null)
                        continue;

                    if (effect.EffectCategory == MajorEffects.Balefiring || effect.EffectCategory == MajorEffects.Warping)
                        continue;

                    EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;

                    detonationsSO.Detonate(effect, data, detonatorType, entry.Target, sourceObject, detonationLevel + data.Intensity);
                    break;

                }


            }

        }
    }
}
