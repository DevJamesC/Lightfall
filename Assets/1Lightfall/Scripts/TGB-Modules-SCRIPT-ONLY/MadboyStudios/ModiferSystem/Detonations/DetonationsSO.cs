using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [CreateAssetMenu(fileName = "DetonationsList", menuName = "MBS/Detonations/ new Detonations List")]
    public class DetonationsSO : ScriptableObject
    {
        [SerializeField]
        private DetonationSO burningDetonation;
        [SerializeField]
        private DetonationSO freezingDetonation;
        [SerializeField]
        private DetonationSO shockingDetonation;
        [SerializeField]
        private DetonationSO drainingDetonation;
        [SerializeField]
        private DetonationSO warpingDetonation;
        [SerializeField]
        private DetonationSO balefiringDetonation;
        public void Detonate(ModifierEffectBase effect, EffectWithIntensityData primerData, MajorEffects detonatorType, ModifierHandler target, GameObject detonatorSource, int detonationLevel = 2, float detonatorRadiusChangePercent = 1, float detonatorDamageChangePercent = 1)
        {
            EffectWithIntensityBase detonateableEffect = effect as EffectWithIntensityBase;
            if (detonateableEffect == null)
                return;

            GetEffect(detonateableEffect.EffectCategory).Detonate(detonateableEffect, primerData, detonatorType, detonationLevel, target, detonatorSource, detonatorDamageChangePercent);
        }

        private DetonationSO GetEffect(MajorEffects effectType)
        {
            switch (effectType)
            {
                case MajorEffects.Burning:
                    return burningDetonation;
                case MajorEffects.Freezing:
                    return freezingDetonation;
                case MajorEffects.Shocking:
                    return shockingDetonation;
                case MajorEffects.Draining:
                    return drainingDetonation;
                case MajorEffects.Warping:
                    return warpingDetonation;
                case MajorEffects.Balefiring:
                    return balefiringDetonation;
            }

            return null;
        }

        public List<MajorEffects> GetReactors(MajorEffects majorEffect)
        {
            return GetEffect(majorEffect).GetReactors(majorEffect);
        }
    }
}
