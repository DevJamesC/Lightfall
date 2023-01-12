using MBS.DamageSystem;
using MBS.StatsAndTags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MBS.AbilitySystem
{
    /// <summary>
    /// Change a damage stat against enemies with the current tag (local to the character who owns the ability)
    /// </summary>
    public class ChangeDamageAgainstTag : AbilityUpgradeBase
    {
        [SerializeField]
        private List<DamageToTagEntry> damageChangeAgainstTags = new List<DamageToTagEntry>();

        public override void Use(AbilityWrapperBase wrapperAbility)
        {

            foreach (var item in damageChangeAgainstTags)
            {
                //define delegate
                Action<DamageData, IDamageable> MyEventHandler = null;
                MyEventHandler = delegate (DamageData damageData, IDamageable damageable)
                {
                    TagHandler tagHandler = damageable.gameObject.GetComponent<TagHandler>();
                    if (tagHandler == null)
                        return;

                    if (tagHandler.ContainsTag(item.Tags))
                    {
                        float baseVal = 0;
                        switch (item.stat)
                        {
                            case ModifiableStats.WeakpointMultiplier:
                                damageData.GetUserData<MBSExtraDamageData>().WeakpointMultiplier+= item.PercentStatChange;// + item.FlatStatChange);
                            break;

                            case ModifiableStats.WeaponDamage:
                                baseVal = damageData.Amount;
                                if (wrapperAbility.DamageSourceType == DamageSourceType.WeaponDamage)
                                    damageData.Amount=damageData.Amount + (baseVal * item.PercentStatChange / 100);// + item.FlatStatChange);
                            break;

                            case ModifiableStats.AbilityDamage:
                                baseVal = damageData.Amount;
                                if (wrapperAbility.DamageSourceType == DamageSourceType.AbilityDamage)
                                    damageData.Amount=damageData.Amount + (baseVal * item.PercentStatChange / 100);// + item.FlatStatChange);
                            break;

                            case ModifiableStats.MeleeDamage:
                                baseVal = damageData.Amount;
                                if (wrapperAbility.DamageSourceType == DamageSourceType.MeleeDamage)
                                    damageData.Amount=damageData.Amount + (baseVal * item.PercentStatChange / 100);// + item.FlatStatChange);
                            break;

                        }
                    }
                };

                //give delegate
                wrapperAbility.ModifierHandler.OnPreDamageEffects += MyEventHandler;

                //set delegate for cleanup if/ when the ability is disposed
                wrapperAbility.OnPreDamageDelegatesForCleanup.Add(MyEventHandler);

            }

        }

        public override void GetStats(List<AbilityUIStat> stats, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            foreach (var item in damageChangeAgainstTags)
            {
                string tagsAsString = "Damage against ";
                foreach (var tag in item.Tags)
                {
                    tagsAsString += tag.Value;

                    //if we are the second to last item, add an "and"
                    if (item.Tags.IndexOf(tag) == item.Tags.Count - 2 && item.Tags.Count - item.Tags.IndexOf(tag) > 0)
                    {
                        tagsAsString += " and ";
                    }
                    else if (item.Tags.IndexOf(tag) < item.Tags.Count)
                    {
                        tagsAsString += ", ";
                    }

                }


                List<AbilityUIStat> results = stats.Where(stat => stat.StatNameDisplayName == tagsAsString).ToList();
                //If we don't have this stat, create it.
                if (results.Count <= 0)
                {
                    stats.Add(new AbilityUIStat()
                    {
                        StatName = StatName.None,
                        StatNameDisplayName = tagsAsString,
                        MaxValue = item.PercentStatChange,
                        CurrentValue = item.PercentStatChange,
                        InitalValue = item.PercentStatChange,
                        ProspectiveValue = item.PercentStatChange
                    });
                }
                else//else handle incrimenting the stat
                {
                    results[0].MaxValue += item.PercentStatChange;
                    if (hasUpgrade)
                    {
                        results[0].CurrentValue += item.PercentStatChange;
                        results[0].ProspectiveValue += item.PercentStatChange;
                    }
                    else if (isProspectiveUpgrade)
                    {
                        results[0].ProspectiveValue += item.PercentStatChange;
                    }

                }
            }


        }




        [Serializable]
        public class DamageToTagEntry
        {

            [Tooltip("NOTE: Weakpoint multiplier does not get divided by 100. The percent value is instead added. eg x1.5 natural modifier + 2 percentStatChange = x2.5 weakpoint multiplier")]
            public ModifiableStats stat;
            [Tooltip("percent is divided by 100 to get floating point value.")]
            public float PercentStatChange;
            //public float FlatStatChange; //we are not handling flat stat change at the moment
            public List<Tag> Tags;

            public DamageToTagEntry()
            {
                stat = ModifiableStats.WeaponDamage;
                Tags = new List<Tag>();
                PercentStatChange = 0;
                //FlatStatChange = 0;
            }
        }

        public enum ModifiableStats
        {
            WeaponDamage,
            AbilityDamage,
            MeleeDamage,
            WeakpointMultiplier
        }
    }
}
