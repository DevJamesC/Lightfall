using MBS.StatsAndTags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MBS.AbilitySystem
{
    /// <summary>
    /// Change a stat local to the ability
    /// </summary>
    public class StatAbilityUpgrade : AbilityUpgradeBase
    {
        [SerializeField, Tooltip("NOTE: Upgrades are applied locally! Since abilites use abilityDamage, recharge, charges, ect, you will need an effect that applies changes to the " +
            "Modifier System in order to actually have the ability increase melee or weapon damage when used.")]
        private List<AbilityStatChangeEntry> statsToChange = new List<AbilityStatChangeEntry>();

        public override void Use(AbilityWrapperBase wrapperAbility)
        {

            foreach (var item in statsToChange)
            {


                switch (item.StatName)
                {
                    //for special case, set here...
                    //case StatName.AbilityCapacity:
                    //customCode
                    //break;              

                    default:
                        wrapperAbility.ChangeStatModifierValue(item.StatName, item.Value, !item.flatValueChange);
                        break;
                }
            }


        }

        public override void GetStats(List<AbilityUIStat> stats, bool hasUpgrade, bool isProspectiveUpgrade)
        {

            foreach (var stat in statsToChange)
            {
                List<AbilityUIStat> results = stats.Where(st => st.StatName == stat.StatName).ToList();
                //If we don't have this stat, create it.
                if (results.Count <= 0)
                {
                    string displayName = "";
                    string displaySuffix = "";
                    switch (stat.StatName)
                    {
                        case StatName.AbilityRecharge: displayName = "Recharge Speed"; displaySuffix = " sec"; break;
                        case StatName.AbilityCapacity: displayName = "Capacity"; break;
                        case StatName.AbilityDamage: displayName = "Damage"; break;
                        case StatName.AbilityRadius: displayName = "Radius"; displaySuffix = " m"; break;
                        case StatName.AbilityDuration: displayName = "Duration"; displaySuffix = " sec"; break;
                        case StatName.AbilityForce: displayName = "Force"; displaySuffix = "N"; break;
                        case StatName.AbilityModifierEffectIntensity: displayName = "Intensity"; displaySuffix = "N"; break;
                        case StatName.DetonationSize: displayName = "Ability Detonation Size"; displaySuffix = "%"; break;
                        case StatName.AbilityArmorEffectiveness: displayName = "Armor Effectiveness"; displaySuffix = "%"; break;
                        case StatName.AbilityShieldEffectiveness: displayName = "Shield Effectiveness"; displaySuffix = "%"; break;
                    }

                    float statVal = hasUpgrade ? stat.Value : 0;
                    float prospectiveValue = (isProspectiveUpgrade || hasUpgrade) ? stat.Value : 0;

                    if (statVal != 0)
                        if (stat.DivideFlatValueBy100 && stat.flatValueChange)
                            statVal = statVal * 100;

                    if (prospectiveValue != 0)
                        if (stat.DivideFlatValueBy100 && stat.flatValueChange)
                            prospectiveValue = prospectiveValue * 100;


                    stats.Add(new AbilityUIStat()
                    {
                        StatName = stat.StatName,
                        StatNameDisplayName = displayName,
                        statValueDisplaySuffix = displaySuffix,
                        MaxValue = statVal,
                        CurrentValue = statVal,
                        InitalValue = statVal,
                        ProspectiveValue = prospectiveValue
                    });
                }
                else//else handle incrimenting the stat
                {

                    float statVal = 0;

                    if (stat.DivideFlatValueBy100 && stat.flatValueChange)
                        statVal = stat.Value * 100;
                    else
                        statVal = stat.Value;

                    if (statVal >= 0)
                        results[0].MaxValue += stat.flatValueChange ? statVal : (results[0].MaxValue * statVal);
                    else
                        results[0].InitalValue += stat.flatValueChange ? statVal : (results[0].MaxValue * statVal);

                    if (hasUpgrade)
                    {
                        results[0].CurrentValue += stat.flatValueChange ? statVal : results[0].CurrentValue * statVal;
                        results[0].ProspectiveValue += stat.flatValueChange ? statVal : results[0].ProspectiveValue * statVal;
                    }
                    else if (isProspectiveUpgrade)
                    {
                        results[0].ProspectiveValue += stat.flatValueChange ? statVal : results[0].ProspectiveValue * statVal;
                    }


                }
            }


        }

    }
}