using MBS.DamageSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class WeakpointCollider : ValuePoolColliderModifier
    {
        [SerializeField, Range(0, 1), Tooltip("If the target has sheilds up, the extra damage will be multiplied (reduced) by this amount.")]
        private float shieldModifier = 1;

        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            OnTakeHit += IncreaseDamage;
        }

        private void OnDisable()
        {
            OnTakeHit -= IncreaseDamage;
        }

        private void IncreaseDamage(DamageData damageData)
        {
            float extraDamage = Mathf.Clamp((damageData.Damage * damageData.WeakpointMultiplier) - damageData.Damage, 0, int.MaxValue);

            if (health.shield.IsAlive)
                extraDamage *= shieldModifier;

            damageData.SetDamage(Mathf.RoundToInt(damageData.Damage + extraDamage));
        }
    }
}
