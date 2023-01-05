
using MBS.ModifierSystem;

namespace MBS.AbilitySystem
{
    public class GetModifierHandlerAbilityEffectOnInitalized : AbilityEffectOnInitalizedLogicBase
    {
        public override void OnInit(AbilityWrapperBase abilityWrapper)
        {
            IAbilityModifierHandler wrapperModifierHandler = abilityWrapper as IAbilityModifierHandler;
            if (wrapperModifierHandler == null)
                return;

            wrapperModifierHandler.ModifierHandler = abilityWrapper.Origin.GetComponent<ModifierHandler>();
        }
    }
}

