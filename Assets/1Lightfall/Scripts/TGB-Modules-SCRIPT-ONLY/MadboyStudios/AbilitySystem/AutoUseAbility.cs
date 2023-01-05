using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class AutoUseAbility : AbilityComponentBase
    {
        [SerializeField]
        private AbilityUseType abilityUseType;

        [SerializeField]
        private float autoCastRate = 2;

        private float remainingRecharge = 0;

        [SerializeField, ReadOnly]
        private float AbilityCooldownRemaining;//used for GUI and debug


        protected override void Start()
        {
            base.Start();
            if (abilityUseType == AbilityUseType.UseOnceOnStart)
                wrappedAbility.TryUse();
        }

        protected override void Update()
        {
            base.Update();

            if (abilityUseType == AbilityUseType.AutoUseByIntervel)
                AutoUse();

            if (abilityUseType == AbilityUseType.UseOnClick)
                UseWithKeyPress();

            AbilityCooldownRemaining = wrappedAbility.RechargeRemaining;//used for GUI display and debug
        }

        private void OnValidate()
        {
            if (Application.isPlaying && ability != null && enabled)
            {
                SetWrappedAbility();
            }

        }

        private void AutoUse()
        {
            if (remainingRecharge <= 0)
            {
                remainingRecharge = autoCastRate;
                wrappedAbility.TryUse();
            }
            else
            {
                remainingRecharge -= Time.deltaTime;
            }
        }

        private void UseWithKeyPress()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                wrappedAbility.TryUse();
            }
        }

        [Serializable]
        protected enum AbilityUseType
        {
            UseOnceOnStart,
            UseOnClick,
            AutoUseByIntervel
        }
    }
}
