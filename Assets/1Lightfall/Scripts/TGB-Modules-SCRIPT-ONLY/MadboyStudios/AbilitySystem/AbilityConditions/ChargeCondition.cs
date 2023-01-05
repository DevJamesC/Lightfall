using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class ChargeCondition : AbilityConditionBase
    {
        public override bool Validate(AbilityWrapperBase wrapperAbility)
        {
            return wrapperAbility.ChargesRemaining > 0;
        }
    }
}
