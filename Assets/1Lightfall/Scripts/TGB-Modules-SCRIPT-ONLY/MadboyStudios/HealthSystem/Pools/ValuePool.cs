using MBS.DamageSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public abstract class ValuePool : MonoBehaviour
    {
        /// <summary>
        /// Returns true if the pool has a value greater than 0. If 0 or below, returns false.
        /// </summary>
        public bool IsAlive { get => currentValue > 0; }

        [SerializeField, Tooltip("The max pool value of the object. Also the starting health.")]
        protected float maxValue;
        [SerializeField, ReadOnly, Tooltip("The current pool value of the object.")]
        protected float currentValue;

        public event Action<DamageData> PreTakeDamage = delegate { };
        public event Action<DamageData> PostTakeDamage = delegate { };
        public event Action<float> PreHeal = delegate { };
        public event Action<float> PostHeal = delegate { };
        public event Action<float, float> ValueChanged = delegate { };


        protected virtual void OnEnable()
        {
            ResetValuePoolComponent();
        }
        public virtual void TakeDamage(DamageData damageData, Collider collider = null)
        {
            if (!IsAlive)
                return;

            PreTakeDamage.Invoke(damageData);

            DecreaseCurrentValue(damageData.Damage);

            PostTakeDamage.Invoke(damageData);
        }

        public virtual void Heal(int value)
        {
            PreHeal.Invoke(value);
            IncreaseCurrentValue(value);
            PostHeal.Invoke(value);
        }

        protected virtual void DecreaseCurrentValue(float value)
        {
            if (value < 0)
                return;

            currentValue -= value;

            if (currentValue < 0)
                currentValue = 0;

            ValueChanged.Invoke(maxValue, currentValue);
        }

        protected virtual void IncreaseCurrentValue(float value)
        {
            if (value < 0)
                return;

            currentValue += value;

            if (currentValue > maxValue)
                currentValue = maxValue;

            ValueChanged.Invoke(maxValue, currentValue);
        }

        protected virtual void ResetValuePoolComponent()
        {
            currentValue = maxValue;
        }

        protected virtual void InvokePreDamageEvent(DamageData damageData) => PreTakeDamage.Invoke(damageData);
        protected virtual void InvokePostDamageEvent(DamageData damageData) => PostTakeDamage.Invoke(damageData);
        protected virtual void InvokePreHealEvent(int value) => PreHeal.Invoke(value);
        protected virtual void InvokePostHealEvent(int value) => PostHeal.Invoke(value);
        protected virtual void InvokeValueChangedEvent(int maxValue, int currentValue) => ValueChanged.Invoke(maxValue, currentValue);

    }
}
