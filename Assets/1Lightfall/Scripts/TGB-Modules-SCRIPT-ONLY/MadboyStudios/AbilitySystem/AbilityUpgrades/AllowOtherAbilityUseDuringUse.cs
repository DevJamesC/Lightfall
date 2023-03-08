using MBS.Lightfall;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Character;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class AllowOtherAbilityUseDuringUse : AbilityUpgradeBase
    {
        [SerializeField, Range(0, 3)]
        private int AdditionalAbilityUses = 0;
        //private AbilityLoadout loadout;
        private LightfallAbilityBase loadout;

        public override void Use(AbilityWrapperBase wrapperAbility)
        {
            //loadout = wrapperAbility.Origin.GetComponent<AbilityLoadout>();
            LightfallAbilityBase[] abilities = wrapperAbility.Origin.GetComponent<UltimateCharacterLocomotion>().GetAbilities<LightfallAbilityBase>();

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].AbilityWrapper.AbilityBase == wrapperAbility.AbilityBase)
                    loadout = abilities[i];
            }

            //loadout = abilities.Where(ability => ability.AbilityWrapper == wrapperAbility).FirstOrDefault();
            if (loadout == null)
            {
                Debug.LogWarning("AllowOtherAbilityUseDuringUse upgrade is applied to a gameobject without an AbilityLoadout. It will have no effect.");
                return;
            }

            wrapperAbility.OnUse += (wrapperAbility) => { loadout.castWhileUsingCount += AdditionalAbilityUses; };
            //wrapperAbility.OnFinishUse += (wrapperAbility) => { loadout.ChangeUseWhileUsing(AdditionalAbilityUses * -1); };
        }

        public override void GetStats(List<AbilityUIStat> stats, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            List<AbilityUIStat> results = stats.Where(stat => stat.StatNameDisplayName == "Ability Use").ToList();
            //If we don't have this stat, create it.
            if (results.Count <= 0)
            {
                float currentValue = 0;
                float prospectiveValue = 0;

                if (hasUpgrade)
                {
                    currentValue += AdditionalAbilityUses;
                    prospectiveValue += AdditionalAbilityUses;
                }
                else if (isProspectiveUpgrade)
                {
                    prospectiveValue += AdditionalAbilityUses;
                }

                stats.Add(new AbilityUIStat()
                {
                    StatName = StatName.None,
                    StatNameDisplayName = "Ability Use",
                    MaxValue = AdditionalAbilityUses,
                    CurrentValue = currentValue,
                    InitalValue = 0,
                    ProspectiveValue = prospectiveValue
                });


            }
            else//else handle incrimenting the stat
            {
                results[0].MaxValue += AdditionalAbilityUses;
                if (hasUpgrade)
                {
                    results[0].CurrentValue += AdditionalAbilityUses;
                    results[0].ProspectiveValue += AdditionalAbilityUses;
                }
                else if (isProspectiveUpgrade)
                {
                    results[0].ProspectiveValue += AdditionalAbilityUses;
                }

            }
        }
    }
}
