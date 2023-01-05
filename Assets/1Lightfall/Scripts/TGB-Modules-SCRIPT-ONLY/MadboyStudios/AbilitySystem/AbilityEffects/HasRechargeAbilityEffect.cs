using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class HasRechargeAbilityEffect : AbilityEffectBase
    {
        [SerializeField]
        private float recharge;

        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);
            abilityWrapper.OnFinishUse += SetRecharge;
        }

        private void SetRecharge(AbilityWrapperBase abilityWrapper)
        {
            //base.Use(abilityWrapper);

            float realRecharge = abilityWrapper.GetStatChange(StatName.AbilityRecharge, recharge, true);
            abilityWrapper.RechargeRemaining = realRecharge;
            abilityWrapper.InitalRecharge = realRecharge;

        }

        public override void OnUpdate(AbilityWrapperBase abilityWrapper)
        {
            base.OnUpdate(abilityWrapper);

            if (abilityWrapper.RechargeRemaining > 0)
                abilityWrapper.RechargeRemaining -= Time.deltaTime;
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityRecharge,
                StatNameDisplayName = "Recharge Speed",
                statValueDisplaySuffix = " Sec",
                InitalValue = recharge,
                CurrentValue = recharge,
                MaxValue = recharge,
                ProspectiveValue = recharge
            });


            return returnVal;
        }
    }
}
