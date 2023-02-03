using MBS.AbilitySystem;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class AbilityUpgradeManager : SingletonMonobehaviorOdinSerialized<AbilityUpgradeManager>
    {
        [SerializeField] private Dictionary<string, AbilityUpgradeProgressData> ProgressionData = new Dictionary<string, AbilityUpgradeProgressData>();

        public AbilityUpgradeProgressData GetProgressionData(string abilityID)
        {
            Initalize(abilityID);

            return ProgressionData[abilityID];
        }

        public void SetProgressionData(string abilityID, AbilityUpgradeProgressData data)
        {
            Initalize(abilityID);
            ProgressionData[abilityID] = data;
        }

        private void Initalize(string abilityID)
        {
            if (ProgressionData == null)
                ProgressionData = new Dictionary<string, AbilityUpgradeProgressData>();

            if (!ProgressionData.ContainsKey(abilityID))
                ProgressionData.Add(abilityID, new AbilityUpgradeProgressData());
        }

        private void OnValidate()
        {
            foreach (var pair in ProgressionData)
            {
                pair.Value.OnValidate();
            }
        }

      
    }
}
