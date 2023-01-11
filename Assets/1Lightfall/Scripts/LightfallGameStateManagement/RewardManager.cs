using Opsive.Shared.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class RewardManager : SingletonMonobehaviorOdinSerialized<RewardManager>
    {
        [SerializeField, ReadOnly] int unusedAbilityPoints;
        [SerializeField] private Dictionary<int, RewardContainer> rewards = new Dictionary<int, RewardContainer>();
        private void OnWaveEnd(int currentWave)
        {
            if (!rewards.ContainsKey(currentWave))
                return;

            rewards[currentWave].GiveReward();
        }

        private void OnEnable()
        {
            Opsive.Shared.Events.EventHandler.RegisterEvent<int>(WaveManager.Instance.gameObject, "OnWaveEnd", OnWaveEnd);
        }

        private void OnDisable()
        {
            Opsive.Shared.Events.EventHandler.UnregisterEvent<int>(WaveManager.Instance.gameObject, "OnWaveEnd", OnWaveEnd);
        }

        private void AwardAbilityPoints(int value)
        {
            unusedAbilityPoints += value;
        }

        public bool TrySpendAbilityPoints(int amountToSpend)
        {
            bool returnVal = unusedAbilityPoints >= amountToSpend;

            if (returnVal)
                unusedAbilityPoints -= amountToSpend;

            return returnVal;
        }



        [Serializable]
        private class RewardContainer
        {
            [SerializeField] private List<RewardBase> rewards = new List<RewardBase>();

            public void GiveReward()
            {
                foreach (var reward in rewards)
                {
                    reward.GiveReward();
                }
            }


            [Serializable]
            private class RewardAbilityPoints : RewardBase
            {
                [SerializeField] private int skillpoints;

                public override void GiveReward()
                {
                    RewardManager.Instance.AwardAbilityPoints(skillpoints);
                }
            }
            [Serializable]
            private class RewardItem : RewardBase
            {
                [SerializeField] private string item;

                public override void GiveReward()
                {
                    Debug.Log($"awarded {item} item");
                }
            }
            [Serializable]
            private class RewardBase
            {
                public virtual void GiveReward()
                {

                }
            }
        }
    }


}
