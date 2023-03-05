using System;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityUpgradeBase : IShallowCloneable<AbilityUpgradeBase>
    {
        /// <summary>
        /// Use() occurs when the upgrade is applied to the ability. This is the equivalent of Start(), and only happens when initalized
        /// </summary>
        /// <param name="wrapperAbility"></param>
        public virtual void Use(AbilityWrapperBase wrapperAbility)
        {

        }

        internal AbilityUpgradeBase GetShallowCopy()
        {
            return (AbilityUpgradeBase)MemberwiseClone();
        }
        /// <summary>
        ///  change max value
        ///  change current and prospective value if upgraded
        ///  change prospective value if un-upgraded and fitting the prospectiveUpgrade
        /// </summary>
        /// <param name="returnVal"></param>
        /// <param name="hasUpgrade"></param>
        /// <param name="isProspectiveUpgrade"></param>
        public virtual void GetStats(List<AbilityUIStat> returnVal, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            Debug.LogWarning($"The ability Upgrade {GetType()} is trying to modify upgrade display data, but it has no override for GetStats.");
        }
    }
}

