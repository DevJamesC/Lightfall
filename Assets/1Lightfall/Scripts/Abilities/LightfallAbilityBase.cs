using MBS.AbilitySystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.UltimateCharacterController.Character.Abilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class LightfallAbilityBase : Ability
    {
        //public override bool IsConcurrent { get => true; }
        protected List<LightfallAbilityBase> lightFallAbilitesEquipped;
        public AbilityBase abilitySO;
        [Tooltip("This will be used if upgradeData cannot be found from a progression manager")]
        public AbilityUpgradeProgressData UpgradeData { get; protected set; }
        [SerializeField] protected SharedResourceType sharedResource;
        public UIDisplayLocationType UIDisplayLocation;
        [HideInInspector] public float sharedRechargeRemaining;
        [HideInInspector] public float sharedRechargeLastMax;
        [HideInInspector] public int sharedChargeRemaining;
        [HideInInspector] public int sharedChargeMax;
        public float sharedRechargePercentRemaining
        {
            get
            {
                return sharedRechargeLastMax == 0 ? 0 : Mathf.Clamp01(sharedRechargeRemaining / sharedRechargeLastMax);
            }
        }

        private AbilityWrapperBase abilityWrapper;
        private bool abilityIsActive;
        private int initalAbilityIndexParameter;
        private string saveDataID;
        private bool disposedFlag;

        public AbilityWrapperBase AbilityWrapper { get => abilityWrapper; }
        public bool CanBeCancled { get => abilityWrapper.CanBeCanceled; }
        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (abilitySO == null)
                Debug.Log($"No ability has been assigned to LightfallAbility on {m_CharacterLocomotion.gameObject}.");

            lightFallAbilitesEquipped = new List<LightfallAbilityBase>();


            foreach (var ability in m_CharacterLocomotion.Abilities)
            {
                LightfallAbilityBase lightfallAbility = ability as LightfallAbilityBase;
                if (lightfallAbility != null)
                {
                    lightFallAbilitesEquipped.Add(lightfallAbility);
                }
            }
            initalAbilityIndexParameter = Index;
            saveDataID = $"{abilitySO.name}{gameObject.name}{Index}";
        }

        public override void Start()
        {
            base.Start();
            UpgradeData = AbilityUpgradeManager.Instance.GetProgressionData(saveDataID);

            SetupAbilityWrapper();

            Scheduler.Schedule(.25f, () =>
            {

                if (sharedResource == SharedResourceType.Charges)
                {
                    foreach (var ability in lightFallAbilitesEquipped)
                    {
                        if (ability.sharedResource == SharedResourceType.Charges && ability.UpgradeData.AbilityUnlocked)
                        {
                            ability.sharedChargeMax += abilityWrapper.MaxCharges;
                            ability.sharedChargeRemaining += abilityWrapper.MaxCharges;
                        }
                    }

                }

            });


        }

        private void SetupAbilityWrapper()
        {
            disposedFlag = false;
            ModifierHandler modifierHandler = m_CharacterLocomotion.gameObject.GetComponent<ModifierHandler>();
            TagHandler tagHandler = m_CharacterLocomotion.gameObject.GetComponent<TagHandler>();

            if (modifierHandler == null)
                Debug.Log($"No modifierHandler component is on {m_CharacterLocomotion.gameObject}. This is required for MBS abilities.");

            if (tagHandler == null)
                Debug.Log($"No tagHandler component is on {m_CharacterLocomotion.gameObject}. This is required for MBS abilities.");

            if (abilityWrapper != null)
                abilityWrapper.DisposeAbilityWrapper();

            abilityWrapper = abilitySO.GetAbilityWrapper(UpgradeData, m_CharacterLocomotion.gameObject, modifierHandler, tagHandler);
            abilityWrapper.OnFinishUse += (abilityWrapperContext) => { abilityIsActive = false; StopAbility(true); };
            abilityWrapper.OnCanceled += (abilityWrapperContext) => { abilityIsActive = false; StopAbility(); };
            abilityWrapper.OnDisposed += (abilityWrapperContext) => { abilityIsActive = false; disposedFlag = true; StopAbility(); };
            abilityIsActive = false;
            UpgradeData.OnAbilityUpgrade += UpgradeData_OnAbilityUpgrade;
        }

        private void UpgradeData_OnAbilityUpgrade()
        {
            SetupAbilityWrapper();

            if (sharedResource == SharedResourceType.Charges)
            {
                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Charges && ability.UpgradeData.AbilityUnlocked)
                    {
                        ability.sharedChargeMax = 0;
                        ability.sharedChargeRemaining = 0;
                    }
                }


                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Charges && ability.UpgradeData.AbilityUnlocked)
                    {
                        ability.sharedChargeMax += abilityWrapper.MaxCharges;
                        ability.sharedChargeRemaining += abilityWrapper.MaxCharges;
                    }
                }
            }
        }

        public override bool CanStartAbility()
        {
            foreach (var ability in lightFallAbilitesEquipped)
            {
                if (!ability.CanBeCancled)
                    return false;
            }


            return base.CanStartAbility();
        }

        public override bool AbilityWillStart()
        {
            if (abilityWrapper == null)
            {
                Debug.Log($"No abilityWrapper was able to be obtained by LightfallAbility on {m_CharacterLocomotion.gameObject}. Use breakpoints to investigate further.");
                return false;
            }

            abilityIsActive = true;


            //check if ability is sharing cooldowns/charges, and if so check that the shared cooldown/charges allows use...
            if (sharedResource == SharedResourceType.Recharge && sharedRechargeRemaining > 0)
                return false;


            if (sharedResource == SharedResourceType.Charges && sharedChargeRemaining <= 0)
                return false;

            //validate that the ability itself has all references and passes all conditions it has internally
            if (!abilityWrapper.ValidateUse())
                return false;

            return base.AbilityWillStart();
        }

        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            if (sharedResource == SharedResourceType.Charges)
            {
                abilityWrapper.MaxCharges = sharedChargeMax;
                abilityWrapper.SetChargesRemaining(sharedChargeRemaining);
            }

            if (!abilityWrapper.TryUse())
                StopAbility();
            else
                Index = 100;
        }

        public override void Update()
        {
            base.Update();
            UpdateAbility();

        }

        public override void InactiveUpdate()
        {
            base.InactiveUpdate();
            UpdateAbility();

        }

        private void UpdateAbility()
        {

            if (abilityWrapper == null)
                return;

            bool updateSharedRecharge = sharedResource == SharedResourceType.Recharge && abilityWrapper.RechargeRemaining > 0;

            abilityWrapper.Update();

            //update the shared recharge of lightfall abilities on this object
            if (updateSharedRecharge)
            {
                sharedRechargeRemaining = abilityWrapper.RechargeRemaining;

                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Recharge)
                        ability.sharedRechargeRemaining = sharedRechargeRemaining;
                }
            }


        }

        public override bool CanStopAbility(bool force)
        {
            if (!force)
            {
                if (!abilityWrapper.CanBeCanceled && !disposedFlag)
                {
                    return false;
                }
            }


            if (disposedFlag)
                disposedFlag = false;

            return base.CanStopAbility(force);
        }

        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (abilityIsActive)
                abilityWrapper.TryUse();

            abilityIsActive = false;
            Index = initalAbilityIndexParameter;

            //update the shared recharges of lightfall abilities on this object
            if (sharedResource == SharedResourceType.Recharge)
            {
                sharedRechargeLastMax = abilityWrapper.InitalRecharge;
                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Recharge)
                        ability.sharedRechargeLastMax = sharedRechargeLastMax;
                }
            }

            //update the shared charges of lightfall abilities on this object
            if (sharedResource == SharedResourceType.Charges)
            {

                sharedChargeRemaining = abilityWrapper.ChargesRemaining;

                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Charges)
                        ability.sharedChargeRemaining = sharedChargeRemaining;
                }
            }

        }

        [Serializable]
        public enum SharedResourceType
        {
            None,
            Recharge,
            Charges
        }

        public enum UIDisplayLocationType
        {
            Any,
            Left,
            Right,
            Center
        }

    }

    public class LightfallAbilityOne : LightfallAbilityBase
    {
    }
    public class LightfallAbilityTwo : LightfallAbilityBase
    {
    }
    public class LightfallAbilityThree : LightfallAbilityBase
    {
    }
    public class LightfallAbilityFour : LightfallAbilityBase
    {
    }
    public class LightfallAbilityFive : LightfallAbilityBase
    {
    }
    public class LightfallAbilitySix : LightfallAbilityBase
    {
    }
}
