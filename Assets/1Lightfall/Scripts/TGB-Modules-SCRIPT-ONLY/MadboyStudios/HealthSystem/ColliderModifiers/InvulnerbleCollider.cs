using MBS.DamageSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class InvulnerbleCollider : ValuePoolColliderModifier
    {

        [SerializeField, Range(0, 1), Tooltip("If the target has sheilds up, this percent of damage will still be allowed through")]
        private float shieldModifier = 0;

        protected override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            OnTakeHit += RemoveDamage;
        }

        private void OnDisable()
        {
            OnTakeHit -= RemoveDamage;
        }

        private void RemoveDamage(DamageData damageData)
        {
            float remainingDamage = 0;

            if (health.shield.IsAlive)
                remainingDamage = damageData.Amount * shieldModifier;
            damageData.Amount = Mathf.RoundToInt(remainingDamage);
        }

    }
}
