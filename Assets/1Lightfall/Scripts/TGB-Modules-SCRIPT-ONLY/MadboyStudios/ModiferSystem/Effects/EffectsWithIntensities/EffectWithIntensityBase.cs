using MBS.AbilitySystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class EffectWithIntensityBase : ModifierEffectBase
    {
        [SerializeField]
        private int intensity;
        [SerializeField]
        public int BaseIntensity { get => intensity; }

        [SerializeField, Tooltip("Value is divided by 100 to get standard floating point value. Tip: Rapid-apply effects like flamethrower should" +
            " have 5% or lower chance to intensify. Slower firing things can have 10-20%")]
        private float chanceToIncreaseIntensity = 10;
        [ReadOnly, SerializeField]
        private float chanceToIncreaseToLevel3Intensity;

        [SerializeReference, BoxGroup("Lists")]
        private List<ModifierFXBase> intensity1FX = new List<ModifierFXBase>();
        [SerializeReference, BoxGroup("Lists")]
        private List<ModifierFXBase> intensity2FX = new List<ModifierFXBase>();
        [SerializeReference, BoxGroup("Lists")]
        private List<ModifierFXBase> intensity3FX = new List<ModifierFXBase>();
        [SerializeReference, BoxGroup("Lists")]
        private List<ModifierFXBase> intensity4FX = new List<ModifierFXBase>();
        [SerializeReference, Tooltip("If intensity is 2, do we activate intensity 1 and 2 FX, or just intensity 2 FX.")]
        private bool addIntensityFX;
        public float ChanceToIncreaseIntensity { get => chanceToIncreaseIntensity; }

        [ReadOnly, SerializeField]
        protected MajorEffects effectCategory;
        public MajorEffects EffectCategory { get => effectCategory; }

        public override void EffectActivated(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);
            //in subclass, call Setup(targetEntry) to setup, and get the intensity of the ability

        }

        public override void EffectRemoved(ModifierEntry targetEntry)
        {
            base.EffectRemoved(targetEntry);

            EffectWithIntensityData data = targetEntry.EffectStateData as EffectWithIntensityData;

            switch (data.Intensity)
            {
                case 1:
                    HandleFXList(targetEntry, 1, false);
                    break;
                case 2:
                    HandleFXList(targetEntry, 2, false);
                    break;
                case 3:
                    HandleFXList(targetEntry, 3, false);
                    break;
                case 4:
                    HandleFXList(targetEntry, 4, false);
                    break;
            }

        }

        public override void EffectUpdate(ModifierEntry targetEntry)
        {
            base.EffectUpdate(targetEntry);
        }

        public override void OnValidate()
        {
            base.OnValidate();
            chanceToIncreaseToLevel3Intensity = chanceToIncreaseIntensity / 2;
        }

        public override List<AbilityUIStat> GetStats()
        {
            return base.GetStats();
        }

        protected virtual int Setup(ModifierEntry targetEntry)
        {
            //If we decide to go with effectAlreadyExistsLogic for each effect, enable the below code.
            //check if the target already has this effect on them. If so, determine what action to take
            //if (effectAlreadyExistsLogic.HandleEffectAlreadyExists(targetEntry, targetEntry.Target.GetModifierEntries(targetEntry.Effect)))
            //return;

            //if an effect of this category exists on the target, remove it before applying this one
            //May change this later to handle some form of "IfAlreadyExists" logic for each type of effect
            List<ModifierEntry> entries = targetEntry.Target.GetModifierEntries(this);
            int highestIntensity = intensity;
            foreach (var entry in entries)
            {
                EffectWithIntensityBase effect = entry.Effect as EffectWithIntensityBase;
                if (entry == targetEntry)
                    continue;

                EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;

                if (effect.effectCategory != effectCategory)
                {
                    //clear any opposite effect, unless the opposite is at an intensity 4.
                    if (MajorEffectsUtils.AreEffectsOpposites(effect.effectCategory, EffectCategory))
                    {
                        if (data.Intensity < 4)
                            entry.RemainingDuration = -1;
                        else
                        {
                            targetEntry.RemainingDuration = -1f;
                            return 0;
                        }
                    }

                    continue;
                }





                entry.RemainingDuration = -1;

                if (data.Intensity > highestIntensity)
                    highestIntensity = data.Intensity;

                //try to intensify
                if (highestIntensity < 3)
                {
                    float intensifyChance = (chanceToIncreaseIntensity + effect.chanceToIncreaseIntensity) / 2;
                    if (highestIntensity == 2)
                        intensifyChance = intensifyChance / 2;

                    int randomRoll = Random.Range(1, 101);

                    if (intensifyChance >= randomRoll)
                        highestIntensity++;

                    //Debug.Log($"rolled {randomRoll}, needed {intensifyChance} or lower");
                }

            }



            if (highestIntensity > 3)
                highestIntensity = 3;

            int highestIntensityBeforeBeforeVulnerbility = highestIntensity;
            entries = targetEntry.Target.GetModifierEntries<VulnerbleToMajorEffect>();
            foreach (var entry in entries)
            {
                if (targetEntry.RemainingDuration < 0)
                    break;

                VulnerbleToMajorEffect effect = entry.Effect as VulnerbleToMajorEffect;
                if (effect == null)
                    continue;

                //remove vulerbility modifier if the incoming ability is of the opposite Major Effect
                if (MajorEffectsUtils.AreEffectsOpposites(effect.VulnerbleTo, EffectCategory))
                {
                    entry.RemainingDuration = -1;
                }

                //increase highest intensity if the incoming ability is of the same Major Effect
                if (effect.VulnerbleTo == EffectCategory)
                {
                    entry.RemainingDuration = -1;
                    highestIntensity++;
                }
            }

            if (highestIntensity > highestIntensityBeforeBeforeVulnerbility)
                highestIntensity = highestIntensityBeforeBeforeVulnerbility + 1;

            //increase duration based on intensity. (other effects, such as increasing damage, is handled by subclasses in their SubclassData class)
            switch (highestIntensity)
            {
                case 0:
                    targetEntry.RemainingDuration = -1f;
                    break;
                case 1:
                    HandleFXList(targetEntry, 1, true);
                    break;
                case 2:
                    targetEntry.RemainingDuration *= 1.15f;
                    HandleFXList(targetEntry, 2, true);
                    break;
                case 3:
                    targetEntry.RemainingDuration *= 1.30f;
                    HandleFXList(targetEntry, 3, true);
                    break;
                case 4:
                    targetEntry.RemainingDuration *= 1.40f;
                    HandleFXList(targetEntry, 4, true);
                    break;
            }
            //Debug.Log("applied with intenstity " + highestIntensity);
            return highestIntensity;
        }

        private void HandleFXList(ModifierEntry entry, int intensity, bool activating)
        {
            if (intensity == 1)
                HandleFX(entry, activating, intensity1FX);

            if (intensity == 2)
            {
                HandleFX(entry, activating, intensity2FX);
                if (addIntensityFX)
                    HandleFX(entry, activating, intensity1FX);
            }

            if (intensity == 3)
            {
                HandleFX(entry, activating, intensity3FX);
                if (addIntensityFX)
                {
                    HandleFX(entry, activating, intensity1FX);
                    HandleFX(entry, activating, intensity2FX);
                }
            }

            if (intensity == 4)
            {
                HandleFX(entry, activating, intensity4FX);
                if (addIntensityFX)
                {
                    HandleFX(entry, activating, intensity1FX);
                    HandleFX(entry, activating, intensity2FX);
                    HandleFX(entry, activating, intensity3FX);
                }

            }

        }

        private void HandleFX(ModifierEntry entry, bool activating, List<ModifierFXBase> list)
        {
            foreach (var fx in list)
            {
                if (activating)
                    fx.Activate(entry);
                else
                    fx.Deactivate(entry);
            }
        }

        public override void ApplyAbilitySystemUpgradesToEntries(ModifierEntry entry, AbilityWrapperBase abilityWrapper)
        {
            EffectWithIntensityData data = entry.EffectStateData as EffectWithIntensityData;
            if (data == null)
                return;

            //get and apply local ability upgrades (exclude detonation size modifier, cause that is calculated when data is constructed)
            float realRadiusModifier = abilityWrapper.GetStatChange(StatName.DetonationSize, 1, false, true, false);
            int additionalIntensityBase = Mathf.RoundToInt(abilityWrapper.GetStatChange(StatName.AbilityModifierEffectIntensity, 0, false)) - 1;

            data.SetDetonationSizeModifier(data.DetonationSizeModifier + realRadiusModifier);
            data.SetBaseIntensity(data.BaseIntensity + additionalIntensityBase);

        }

    }

    public class EffectWithIntensityData : EffectStateData
    {
        public int BaseIntensity { get; private set; }
        public int Intensity { get; private set; }

        public float DetonationSizeModifier { get; private set; }

        /// <summary>
        /// detonationSize should be a floating point percent (eg. 50% increase is .5).
        /// </summary>
        /// <param name="baseIntensity"></param>
        /// <param name="detonationSize"></param>
        public EffectWithIntensityData(int baseIntensity, float detonationSizeModifier)
        {
            this.BaseIntensity = baseIntensity;
            Intensity = baseIntensity;
            DetonationSizeModifier = detonationSizeModifier;
        }

        public void SetBaseIntensity(int newBaseIntensity)
        {
            int diff = Intensity - BaseIntensity;
            BaseIntensity = newBaseIntensity;
            Intensity = BaseIntensity + diff;
        }

        public void SetDetonationSizeModifier(float newDetonationSizeModifier)
        {
            DetonationSizeModifier = newDetonationSizeModifier;
        }
    }

    public enum MajorEffects
    {
        Burning,
        Freezing,
        Shocking,
        Draining,
        Warping,
        Balefiring
    }

    public static class MajorEffectsUtils
    {
        public static bool AreEffectsOpposites(MajorEffects firstEffect, MajorEffects secondEffect)
        {
            switch (firstEffect)
            {
                case MajorEffects.Burning:
                    return secondEffect == MajorEffects.Freezing;
                case MajorEffects.Freezing:
                    return secondEffect == MajorEffects.Burning;
                case MajorEffects.Shocking:
                    return secondEffect == MajorEffects.Draining;
                case MajorEffects.Draining:
                    return secondEffect == MajorEffects.Shocking;
                case MajorEffects.Warping:
                    return secondEffect == MajorEffects.Balefiring;
                case MajorEffects.Balefiring:
                    return secondEffect == MajorEffects.Warping;
            }

            return false;
        }
    }
}