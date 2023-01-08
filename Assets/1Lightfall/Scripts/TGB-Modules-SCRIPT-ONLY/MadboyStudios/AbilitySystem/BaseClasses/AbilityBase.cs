using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MBS.AbilitySystem
{
    [CreateAssetMenu(menuName = "MBS/AbilitySystem/ Create New Ability", fileName = "New Ability")]
    public class AbilityBase : ScriptableObject
    {
        [SerializeField]
        private Sprite icon;
        public Sprite Icon { get => icon; }
        [SerializeField]
        private string title;
        public string Title { get => title; }
        [SerializeField, TextArea]
        private string description;
        public string Description { get => description; }
        public List<AbilityConditionBase> AbilityConditions { get => abilityConditions; }
        public List<AbilityEffectBase> AbilityEffects { get => abilityEffects; }

        public List<AbilityFXBase> OnActivateFX { get => onActivateFX; }

        public AbilityType AbilityType { get => abilityType; }
        public float TimeToUse { get => timeToUse; }

        [SerializeField]
        protected AbilityUpgradeProgressionContainer possibleUpgrades;
        public AbilityUpgradeProgressionContainer PossibleUpgrades { get => possibleUpgrades; }
        [SerializeField]
        protected AbilityType abilityType;
        [SerializeField, Tooltip("The time from when the ability starts till the effects are triggered. Useful for syncing up with animations."), ShowIf("IsNotPassive")]
        protected float timeToUse;

        [SerializeReference, ShowIf("IsNotPassive")]
        protected List<AbilityFXBase> onActivateFX;

        //[SerializeField, ShowIf("IsNotPassive")]
        //protected List<AbilityFXBase> onDisableFX;
        [SerializeReference]
        protected List<AbilityConditionBase> abilityConditions = new List<AbilityConditionBase>();
        [SerializeReference]
        protected List<AbilityEffectBase> abilityEffects = new List<AbilityEffectBase>();

        private bool IsNotPassive//Used for GUI only
        {
            get { return abilityType != AbilityType.Passive; }
        }


        /// <summary>
        /// Call this to recieve an "instance" of this ability
        /// </summary>
        /// <param name="upgradeData"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public virtual AbilityWrapperBase GetAbilityWrapper(AbilityUpgradeProgressData upgradeData, GameObject origin, ModifierHandler originHandler, TagHandler originTags)
        {
            return new AbilityWrapperBase(this, upgradeData, origin, originHandler, originTags);
        }

        public List<AbilityUpgradeBase> GetActiveUpgrades(AbilityUpgradeProgressData upgradeData)
        {
            return possibleUpgrades.GetActiveUpgrades(upgradeData);
        }

        internal List<AbilityUIStat> GetStats(AbilityUpgradeProgressData currentUpgrades, string prospectiveUpgrade)
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            //get inital stats
            foreach (var effect in abilityEffects)
            {
                returnVal.AddRange(effect.GetStats());
            }

            possibleUpgrades.GetStats(returnVal, currentUpgrades, prospectiveUpgrade);

            return returnVal;
        }

        public void OnValidate()
        {
            foreach (var condition in abilityConditions)
            {
                condition.OnValidate();
            }

            foreach (var effect in abilityEffects)
            {
                effect.OnValidate();
            }

            possibleUpgrades.OnValidate();
        }

    }
    public enum AbilityState
    {
        StartingUp,//ability is being used, but no effects have been used
        InUse, //effects are being used
        InUseInBackground,//this is used for OnOffActivatables and Passives. It means that the ability isn't really inactive, nor is it technically active
        Deactivating,//this is used for OnOffActivatables. It means that the ability is being used, but it is turning off.
        Inactive,

    }
    public enum AbilityType
    {
        StandardActivatable,
        OnOffActivatable,
        Passive
    }

    [Serializable]
    public class AbilityUpgradeProgressionContainer
    {
        [TabGroup("Group1", "$upgrade1Subclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade1;

        [TabGroup("Group2", "$upgrade2Subclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade2;


        [TabGroup("Group3", "$upgrade3aSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade3a;

        [TabGroup("Group3", "$upgrade3bSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade3b;

        [TabGroup("Group4", "$upgrade4aSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade4a;


        [TabGroup("Group4", "$upgrade4bSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade4b;

        [TabGroup("Group5", "$upgrade5aSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade5a;

        [TabGroup("Group5", "$upgrade5bSubclass")]
        public AbilityUpgradeProgressionContainerEntry Upgrade5b;

        //Used in the GUI only
        private string upgrade1Subclass;
        private string upgrade2Subclass;
        private string upgrade3aSubclass;
        private string upgrade3bSubclass;
        private string upgrade4aSubclass;
        private string upgrade4bSubclass;
        private string upgrade5aSubclass;
        private string upgrade5bSubclass;

        public List<AbilityUpgradeBase> ToList()
        {
            List<AbilityUpgradeBase> returnVal = new List<AbilityUpgradeBase>();

            returnVal.AddRange(Upgrade1.Effects);
            returnVal.AddRange(Upgrade2.Effects);
            returnVal.AddRange(Upgrade3a.Effects);
            returnVal.AddRange(Upgrade3b.Effects);
            returnVal.AddRange(Upgrade4a.Effects);
            returnVal.AddRange(Upgrade4b.Effects);
            returnVal.AddRange(Upgrade5a.Effects);
            returnVal.AddRange(Upgrade5b.Effects);

            return returnVal;
        }

        public void OnValidate()
        {

            upgrade1Subclass = FormatUpgradeToTitle(Upgrade1, "1");
            upgrade2Subclass = FormatUpgradeToTitle(Upgrade2, "2");
            upgrade3aSubclass = FormatUpgradeToTitle(Upgrade3a, "3a");
            upgrade3bSubclass = FormatUpgradeToTitle(Upgrade3b, "3b");
            upgrade4aSubclass = FormatUpgradeToTitle(Upgrade4a, "4a");
            upgrade4bSubclass = FormatUpgradeToTitle(Upgrade4b, "4b");
            upgrade5aSubclass = FormatUpgradeToTitle(Upgrade5a, "5a");
            upgrade5bSubclass = FormatUpgradeToTitle(Upgrade5b, "5b");
        }

        public string FormatUpgradeToTitle(AbilityUpgradeProgressionContainerEntry upgradeContainer, string upgradeLevel)
        {
            List<AbilityUpgradeBase> upgradeEffects = upgradeContainer.Effects;
            string upgradesString = "";
            if (upgradeEffects == null)
                upgradesString = "No Effects Added!";
            else if (upgradeEffects.Count == 0)
                upgradesString = "No Effects Added!";

            //Removed for berevity. May be re-added and refactored later
            //if (upgradeEffects != null)
            //{
            //    foreach (var upgrade in upgradeEffects)
            //    {
            //        if (upgrade == null)
            //        {
            //            upgradesString += "Empty ";
            //            continue;
            //        }

            //        upgradesString += Regex.Replace(upgrade.GetType().Name, "AbilityUpgrade", "");

            //        //If we have more than two elements
            //        if (upgradeEffects.Count > 2)
            //        {
            //            //check if this element is not the last, or second to last element
            //            if (!upgrade.Equals(upgradeEffects[upgradeEffects.Count - 2]) && !upgrade.Equals(upgradeEffects[upgradeEffects.Count - 1]))
            //            {
            //                upgradesString += ", ";
            //            }
            //            else if (upgrade.Equals(upgradeEffects[upgradeEffects.Count - 2]))//else check if this element is the second to last element
            //            {
            //                upgradesString += ", "; //" and "; //removed for berevity
            //            }
            //            else if (upgrade.Equals(upgradeEffects[upgradeEffects.Count - 1]))//else check if this element is the last element
            //            {

            //            }
            //        }
            //        else if (upgradeEffects.Count > 1)//else if we have two elements
            //        {
            //            if (upgrade.Equals(upgradeEffects[upgradeEffects.Count - 2]))//else check if this element is the second to last element
            //            {
            //                upgradesString += ", "; //" and "; //removed for berevity
            //            }
            //            else if (upgrade.Equals(upgradeEffects[upgradeEffects.Count - 1]))//else check if this element is the last element
            //            {

            //            }
            //        }
            //    }
            //}

            string returnVal = $"Upgrade {upgradeLevel}: \"{upgradeContainer.Title}\" {upgradesString}";

            return returnVal;
        }

        public List<AbilityUpgradeBase> GetActiveUpgrades(AbilityUpgradeProgressData data)
        {
            List<AbilityUpgradeBase> returnVal = new List<AbilityUpgradeBase>();

            if (!data.AbilityUnlocked)
                return returnVal;

            if (data.Upgrade1)
                returnVal.AddRange(Upgrade1.Effects);
            if (data.Upgrade2)
                returnVal.AddRange(Upgrade2.Effects);
            if (data.Upgrade3a)
                returnVal.AddRange(Upgrade3a.Effects);
            if (data.Upgrade3b)
                returnVal.AddRange(Upgrade3b.Effects);
            if (data.Upgrade4a)
                returnVal.AddRange(Upgrade4a.Effects);
            if (data.Upgrade4b)
                returnVal.AddRange(Upgrade4b.Effects);
            if (data.Upgrade5a)
                returnVal.AddRange(Upgrade5a.Effects);
            if (data.Upgrade5b)
                returnVal.AddRange(Upgrade5b.Effects);

            return returnVal;
        }

        internal void GetStats(List<AbilityUIStat> returnVal, AbilityUpgradeProgressData currentUpgrades, string prospectiveUpgrade)
        {
            Upgrade1.GetStats(returnVal, currentUpgrades.Upgrade1, prospectiveUpgrade == "1");
            Upgrade2.GetStats(returnVal, currentUpgrades.Upgrade2, prospectiveUpgrade == "2");
            Upgrade3a.GetStats(returnVal, currentUpgrades.Upgrade3a, prospectiveUpgrade == "3a");
            Upgrade3b.GetStats(returnVal, currentUpgrades.Upgrade3b, prospectiveUpgrade == "3b");
            Upgrade4a.GetStats(returnVal, currentUpgrades.Upgrade4a, prospectiveUpgrade == "4a");
            Upgrade4b.GetStats(returnVal, currentUpgrades.Upgrade4b, prospectiveUpgrade == "4b");
            Upgrade5a.GetStats(returnVal, currentUpgrades.Upgrade5a, prospectiveUpgrade == "5a");
            Upgrade5b.GetStats(returnVal, currentUpgrades.Upgrade5b, prospectiveUpgrade == "5b");
        }

        [Serializable]
        public class AbilityUpgradeProgressionContainerEntry
        {
            public Sprite Icon;
            public string Title;
            [TextArea]
            public string Description;
            [SerializeReference]
            public List<AbilityUpgradeBase> Effects;

            internal void GetStats(List<AbilityUIStat> returnVal, bool hasUpgrade, bool isProspectiveUpgrade)
            {
                foreach (var upgradeEffect in Effects)
                {
                    //change max value
                    //change current and prospective value if upgraded
                    //change prospective value if un-upgraded and fitting the prospectiveUpgrade
                    upgradeEffect.GetStats(returnVal, hasUpgrade, isProspectiveUpgrade);
                }
            }
        }
    }

    public class AbilityUIStat
    {
        public StatName StatName;
        public string StatNameDisplayName;//if set, will display this text instead of StatName.
        public string statValueDisplaySuffix;
        public Sprite EffectIcon;
        //public string Description; //not used yet (there is no tooltip or way to navigate to stats atm)
        public float InitalValue;
        public float MaxValue;
        public float CurrentValue;
        public float ProspectiveValue;
        //add an image, so it can return an "effect" that isn't necessarily a stat increase (such as "bleed effect", or fire primer/ detonator)?
    }
}

