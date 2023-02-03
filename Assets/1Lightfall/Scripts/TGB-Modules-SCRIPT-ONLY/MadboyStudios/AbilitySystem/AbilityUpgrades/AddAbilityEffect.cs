using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class AddAbilityEffect : AbilityUpgradeBase
    {
        [SerializeReference] protected AbilityEffectBase abilityEffectToAdd;

        public override void Use(AbilityWrapperBase wrapperAbility)
        {
            if (abilityEffectToAdd == null)
            {
                Debug.Log($"ability {wrapperAbility.AbilityBase.name} has an upgrade to add an ability effect... but the effect to add is null");
                return;
            }

            wrapperAbility.AddAbilityEffect(abilityEffectToAdd); 
            
        }

        public override void GetStats(List<AbilityUIStat> returnVal, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            if (abilityEffectToAdd == null)
                return;

            if (!hasUpgrade && !isProspectiveUpgrade)
                return;

            List<AbilityUIStat> abilityEffectStats = abilityEffectToAdd.GetStats();
            if (isProspectiveUpgrade && !hasUpgrade)
            {
                foreach (AbilityUIStat modifierStat in abilityEffectStats)
                {
                    modifierStat.InitalValue = .01f;
                    modifierStat.CurrentValue = .01f;
                }
            }

            returnVal.AddRange(abilityEffectStats);
        }
    }
}
