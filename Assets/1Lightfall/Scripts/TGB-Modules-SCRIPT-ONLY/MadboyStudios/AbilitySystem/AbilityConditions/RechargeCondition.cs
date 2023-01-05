
namespace MBS.AbilitySystem
{
    public class RechargeCondition : AbilityConditionBase
    {
        public override bool Validate(AbilityWrapperBase wrapperAbility)
        {
            return wrapperAbility.RechargeRemaining <= 0;
        }
    }
}





