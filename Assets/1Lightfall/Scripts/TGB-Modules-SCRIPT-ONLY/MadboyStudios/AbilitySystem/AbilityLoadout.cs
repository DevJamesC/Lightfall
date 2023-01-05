using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class AbilityLoadout : MonoBehaviour
    {
        [SerializeField]
        protected bool abilitesShareRecharge;
        [SerializeField]
        protected bool abilitiesShareCharge;
        [SerializeField]
        protected List<AbilityAndUpgradePair> abilities = new List<AbilityAndUpgradePair>();
        public List<AbilityAndUpgradePair> Abilities { get { return abilities; } }
        [SerializeField, ReadOnly, TextArea(4, 10)]
        private string DebugAbilityStateInfo;

        protected List<AbilityWrapperBase> wrappedAbilities;
        protected List<AbilityWrapperBase> chargeBasedAbilites;//Subset of wrappedAbilites which has MaxCharges>0
        protected List<AbilityWrapperBase> abilitesInUse;
        public event Action AbilitiesChanged = delegate { };//This triggers in invoke. Eventually we will need a more proper way to "upgrade" abilites.
        private TagHandler tagHandler;
        protected ModifierHandler modifierHandler;

        protected float initalRecharge;
        protected float rechargeRemaining;
        protected float RechargeRemaining
        {
            get => rechargeRemaining;
            set
            {
                rechargeRemaining = value;

                if (rechargeRemaining > initalRecharge)
                    initalRecharge = rechargeRemaining;

                if (rechargeRemaining <= 0)
                {
                    rechargeRemaining = 0;
                    initalRecharge = 0;
                }
            }
        }

        protected int maxCharges;
        private int chargesRemaining;
        protected int ChargesRemaining
        {
            get => chargesRemaining;
            set
            {
                chargesRemaining = value;

                if (chargesRemaining > maxCharges)
                    chargesRemaining = maxCharges;

                if (chargesRemaining < 0)
                    chargesRemaining = 0;

                if (abilitiesShareCharge)
                {
                    foreach (var ability in chargeBasedAbilites)
                    {
                        ability.SetChargesRemaining(chargesRemaining);
                    }
                }

            }
        }


        protected int castWhileUsingCount;

        protected void Awake()
        {
            wrappedAbilities = new List<AbilityWrapperBase>();
            chargeBasedAbilites = new List<AbilityWrapperBase>();
            abilitesInUse = new List<AbilityWrapperBase>();
            tagHandler = GetComponent<TagHandler>();
            modifierHandler = GetComponent<ModifierHandler>();
        }

        // Start is called before the first frame update
        protected void Start()
        {
            Setup();
            foreach (var ability in abilities)
            {
                ability.Upgrades.OnAbilityUpgrade += Setup;
            }
        }

        protected void Setup()
        {
            initalRecharge = 0;
            RechargeRemaining = 0;
            maxCharges = 0;
            ChargesRemaining = 0;
            castWhileUsingCount = 0;

            foreach (var ability in abilities)
            {
                SetWrappedAbility(ability);

            }

            //Applies the ChargesRemaining to all charge based abilites, now that we would have added them all to our list
            ChargesRemaining = ChargesRemaining;
        }


        public void TryUseAbility(int index)
        {
            if (wrappedAbilities.Count <= index || 0 > index)
                return;

            AbilityWrapperBase abilityToUse = wrappedAbilities[index];

            //check if we are already using an ability. If so, check that the ability we are using is not the ability we wish to use (such as cancelling a channel)
            //If we are trying to use a new ability, check if we can use an ability while using another ability.
            //If we are trying to use a new ability, check if we are allowed to cancel our current ability.

            if (abilitesInUse.Count > 0 && !abilitesInUse.Contains(abilityToUse))
            {
                bool canReturn = true;
                if (abilitesInUse[0].CancelableOnOtherAbilityCast)
                {
                    abilitesInUse[0].TryUse();//This should handle cancelling the ability, if the effects can cancel themself OnUseWhileUsing()
                    canReturn = false;
                }

                if (abilityToUse.AbilityState != AbilityState.InUseInBackground)
                {
                    if (castWhileUsingCount > 0)
                        ChangeUseWhileUsing(-1);
                    else if (canReturn)
                        return;
                }

            }



            //if we are on a shared recharge and not a charge based ability, and not an OnOff ability InUseInBackground, then check if we have recharge remaining. If so, return.
            if (abilitesShareRecharge && !chargeBasedAbilites.Contains(abilityToUse) && abilityToUse.AbilityState != AbilityState.InUseInBackground && RechargeRemaining > 0)
            {
                return;
            }

            //check that no other abilites are currently starting up or deactivating (because we can't/ shouldn't play two animations at once)
            //This bascially makes sure that OnOff abilites are not in a state of transition.
            foreach (var ability in wrappedAbilities)
            {
                if (ability.AbilityState == AbilityState.StartingUp || ability.AbilityState == AbilityState.Deactivating)
                    return;

            }

            if (abilityToUse.TryUse())
            {
                if (abilityToUse.AbilityBase.AbilityType == AbilityType.StandardActivatable)
                    if (!abilitesInUse.Contains(abilityToUse))
                        abilitesInUse.Add(abilityToUse);
            }
        }





        // Update is called once per frame
        protected virtual void Update()
        {
            UpdateWrappedAbilites();
            UpdateAbilityStateDebugString();

            if (rechargeRemaining > 0)
                RechargeRemaining -= Time.deltaTime;
        }

        private void UpdateWrappedAbilites()
        {
            for (int i = 0; i < wrappedAbilities.Count; i++)
            {
                wrappedAbilities[i].Update();
            }
        }


        private void UpdateAbilityStateDebugString()
        {
            DebugAbilityStateInfo = "";

            foreach (var ability in wrappedAbilities)
            {
                string rechargeOrCooldown = (ability.MaxCharges > 0) ?
                   (abilitiesShareCharge) ? $"Charges: {ChargesRemaining}" : $"Charges: {ability.ChargesRemaining}" :
                    (abilitesShareRecharge) ? $"Recharge: {RechargeRemaining}" : $"Recharge: {ability.RechargeRemaining}";

                DebugAbilityStateInfo += $"{ability.AbilityBase.name} - {ability.AbilityState} - {rechargeOrCooldown} \n";
            }

        }


        /// <summary>
        /// Used by AllowOtherAbilitesDuringUse upgrade
        /// </summary>
        /// <param name="increaseVal"></param>
        public void ChangeUseWhileUsing(int increaseVal)
        {
            castWhileUsingCount += increaseVal;

            if (castWhileUsingCount < 0)
                castWhileUsingCount = 0;
        }

        protected void SetWrappedAbility(AbilityAndUpgradePair baseAbility)
        {
            //If we have a current ability of this type, then dispose of it before generating a new wrapper.
            AbilityWrapperBase currentWrappedAbility = GetWrappedAbilityFromBase(baseAbility.AbilitySO);
            if (currentWrappedAbility != null)
            {
                currentWrappedAbility.DisposeAbilityWrapper();
                wrappedAbilities.Remove(currentWrappedAbility);
            }

            //Get the new ability wrapper
            AbilityWrapperBase newWrappedAbility = baseAbility.AbilitySO.GetAbilityWrapper(baseAbility.Upgrades, gameObject, modifierHandler, tagHandler);
            if (newWrappedAbility.AbilityBase.AbilityType == AbilityType.StandardActivatable)
            {
                newWrappedAbility.OnFinishUse += (abilityWrapper) => { if (abilitesInUse.Contains(abilityWrapper)) abilitesInUse.Remove(abilityWrapper); };//auto-remove from "In Use" list when finished
                newWrappedAbility.OnCanceled += (abilityWrapper) => { if (abilitesInUse.Contains(abilityWrapper)) abilitesInUse.Remove(abilityWrapper); };//auto-remove from "In Use" list when canceled
                newWrappedAbility.OnDisposed += (abilityWrapper) => { if (abilitesInUse.Contains(abilityWrapper)) abilitesInUse.Remove(abilityWrapper); };//auto-remove from "In Use" list when disposed
            }


            //Setting up shared charges
            if (newWrappedAbility.MaxCharges > 0 && abilitiesShareCharge)
            {
                maxCharges += newWrappedAbility.MaxCharges;
                ChargesRemaining += newWrappedAbility.ChargesRemaining;
                newWrappedAbility.OnChargesChanged += (newVal) => { ChargesRemaining = newVal; };//auto decriment charges
                chargeBasedAbilites.Add(newWrappedAbility);
            }
            else if (abilitesShareRecharge)//setting up shared recharge
            {
                newWrappedAbility.OnFinishUse += (abilityWrapper) =>
                {
                    RechargeRemaining += abilityWrapper.RechargeRemaining;
                    abilityWrapper.RechargeRemaining = 0;
                };
            }



            wrappedAbilities.Add(newWrappedAbility);
        }

        protected AbilityWrapperBase GetWrappedAbilityFromBase(AbilityBase baseAbility)
        {

            for (int i = 0; i < wrappedAbilities.Count; i++)
            {
                if (wrappedAbilities[i].AbilityBase == baseAbility)
                    return wrappedAbilities[i];
            }

            return null;
        }

        private void OnValidate()
        {
            RemoveDuplicateAbilitesFromList();

            foreach (var abilityUpPair in abilities)
            {
                if (abilityUpPair.AbilitySO == null)
                    continue;

                string levelText = abilityUpPair.Upgrades.AbilityUnlocked ? $"LVL: {abilityUpPair.Upgrades.GetUIUpgradeLevel()}" : "Not Unlocked";
                abilityUpPair.InfoBoxMessage = $"{abilityUpPair.AbilitySO.name}: {levelText}";
            }

            //Re unwrap abilities if there is a change
            if (Application.isPlaying && wrappedAbilities != null)
            {
                Setup();
                AbilitiesChanged.Invoke();
            }
        }

        private void RemoveDuplicateAbilitesFromList()
        {
            List<AbilityAndUpgradePair> toRemove = new List<AbilityAndUpgradePair>();

            foreach (var abilityUpPair in abilities)
            {
                if (toRemove.Contains(abilityUpPair))
                    continue;

                List<AbilityAndUpgradePair> query = abilities.Where(abilityUpgradePair => abilityUpgradePair.AbilitySO == abilityUpPair.AbilitySO).ToList();

                if (query.Contains(abilityUpPair))
                    query.Remove(abilityUpPair);

                toRemove.AddRange(query);
            }

            foreach (var abilityToRemove in toRemove)
            {
                abilities.Remove(abilityToRemove);
            }

            if (toRemove.Count > 0)
                Debug.LogWarning($"Automatically removed duplicate ability \"{toRemove[0].AbilitySO.name}\" from {gameObject.name}'s loadout.");
        }

        private void OnDestroy()
        {
            foreach (var ability in abilities)
            {
                ability.Upgrades.OnAbilityUpgrade -= Setup;
            }
        }
    }

    [Serializable]
    public class AbilityAndUpgradePair
    {
        public AbilityBase AbilitySO;
        [InfoBox("$InfoBoxMessage", InfoMessageType.None)]
        public AbilityUpgradeProgressData Upgrades;
        [HideInInspector]
        public string InfoBoxMessage;
    }
}