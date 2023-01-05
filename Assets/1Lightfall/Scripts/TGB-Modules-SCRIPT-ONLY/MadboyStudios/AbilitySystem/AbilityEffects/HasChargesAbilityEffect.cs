using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class HasChargesAbilityEffect : AbilityEffectBase
    {
        [SerializeField]
        private int maxCharges;
        [SerializeField, Tooltip("Does this ability consume a charge when used, or will a different ability effect handle decrimenting the charges (eg, a grenade/ grenade launcher equippableAbility)")]
        private bool consumeChargeOnUse = true;
        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);
            if (consumeChargeOnUse)
                abilityWrapper.OnUse += DecreaseCharges;

            abilityWrapper.BaseMaxCharges = maxCharges;

            abilityWrapper.SetChargesRemaining(abilityWrapper.BaseMaxCharges);

        }

        private void DecreaseCharges(AbilityWrapperBase abilityWrapper)
        {
            if (abilityWrapper.ChargesRemaining > 0)
                abilityWrapper.ChangeChargesRemaining(-1);
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityCapacity,
                StatNameDisplayName = "Capacity",
                InitalValue = maxCharges,
                CurrentValue = maxCharges,
                MaxValue = maxCharges,
                ProspectiveValue = maxCharges
            });


            return returnVal;
        }
    }
}
