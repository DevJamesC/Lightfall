using MBS.StatsAndTags;
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
        private AbilityLoadout loadout;

        public override void Use(AbilityWrapperBase wrapperAbility)
        {
            loadout = wrapperAbility.Origin.GetComponent<AbilityLoadout>();
            if (loadout == null)
            {
                Debug.LogWarning("AllowOtherAbilityUseDuringUse upgrade is applied to a gameobject without an AbilityLoadout. It will have no effect.");
                return;
            }

            wrapperAbility.OnUse += (wrapperAbility) => { loadout.ChangeUseWhileUsing(AdditionalAbilityUses); };
            wrapperAbility.OnFinishUse += (wrapperAbility) => { loadout.ChangeUseWhileUsing(AdditionalAbilityUses * -1); };
        }

        public override void GetStats(List<AbilityUIStat> stats, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            List<AbilityUIStat> results = stats.Where(stat => stat.StatNameDisplayName == "Ability Use").ToList();
            //If we don't have this stat, create it.
            if (results.Count <= 0)
            {
                stats.Add(new AbilityUIStat()
                {
                    StatName = StatName.None,
                    StatNameDisplayName = "Ability Use",
                    MaxValue = AdditionalAbilityUses,
                    CurrentValue = AdditionalAbilityUses,
                    InitalValue = AdditionalAbilityUses,
                    ProspectiveValue = AdditionalAbilityUses
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
