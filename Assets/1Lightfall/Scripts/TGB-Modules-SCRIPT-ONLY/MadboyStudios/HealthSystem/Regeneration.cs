using MBS.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class Regeneration : MonoBehaviour
    {
        [SerializeField]
        private ValuePool targetPool;
        [SerializeField]
        private ValuePool[] canInterruptRegen;
        [SerializeField, Tooltip("The amount gained each tick.")]
        private int regenAmount;
        [SerializeField, Tooltip("How often a tick occurs, in seconds.")]
        private float regenRate;
        [SerializeField, Tooltip("The time before regeneration resumes after being interrupted.")]
        private float regenDelay;

        private float regenDelayCount;
        private float regenRateCount;

        private void Awake()
        {
            if (targetPool == null)
                targetPool = GetComponent<ValuePool>();

            if (canInterruptRegen == null || canInterruptRegen.Length == 0)
                canInterruptRegen = new ValuePool[1] { targetPool };

            regenDelayCount = regenDelay;
        }

        private void Start()
        {
            foreach (var pool in canInterruptRegen)
            {
                pool.PostTakeDamage += InterruptRegen;
            }
        }

        private void OnDestroy()
        {
            foreach (var pool in canInterruptRegen)
            {
                pool.PostTakeDamage -= InterruptRegen;
            }
        }


        private void Update()
        {
            if (regenDelayCount <= 0)
            {
                if (regenRateCount <= 0)
                {
                    targetPool.Heal(regenAmount);
                    regenRateCount = regenRate;
                }
                else
                {
                    regenRateCount -= Time.deltaTime;
                }
            }
            else
            {
                regenDelayCount -= Time.deltaTime;
            }
        }

        private void InterruptRegen(DamageData damageData)
        {
            if (damageData.Amount > 0)
            {
                regenDelayCount = regenDelay;
                regenRateCount = regenRate;
            }
        }

    }
}
