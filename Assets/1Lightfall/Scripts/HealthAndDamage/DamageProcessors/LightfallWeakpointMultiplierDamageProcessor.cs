using Opsive.UltimateCharacterController.Traits.Damage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    [CreateAssetMenu(fileName = "LightfallDamageProcessor", menuName = "MBS/Damage Processors/Lightfall Weakpoint Multiplier Damage Processor")]
    public class LightfallWeakpointMultiplierDamageProcessor : DamageProcessor
    {
        [SerializeField, Tooltip("The highest multiplier will be taken. Additional functionality can be added later")]
        private float weakpointMultiplier;
        public override void Process(IDamageTarget target, DamageData damageData)
        {

            CalculateMultiplier(target, damageData);
            target.Damage(damageData);
        }

        private void CalculateMultiplier(IDamageTarget target, DamageData damageData)
        {
            LightfallHealth health = target as LightfallHealth;
            if (health == null)
                return;

            //check if we hit a weakpoint
            float colliderDamageMult = health.GetColliderMultiplier(damageData);
            if (colliderDamageMult <= 1) return;

            //if the multiplier is less than our processor multiplier, multiply the damage by our multiplier and set "multiply by weakpoint Damage" to false
            if (colliderDamageMult >= weakpointMultiplier) return;

            damageData.Amount *= weakpointMultiplier;
            damageData.GetUserData<LightfallDamageData>().UseColliderDamageMultiplier = false;
        }
    }
}
