using MBS.AbilitySystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.Shared.Game;
using Opsive.Shared.Input;
using Opsive.UltimateCharacterController.Character.Abilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class LightfallAbilityBase : Ability
    {
        public override bool IsConcurrent { get => true; }
        protected List<LightfallAbilityBase> lightFallAbilitesEquipped;
        public AbilityBase abilitySO;
        [Tooltip("This will be used if upgradeData cannot be found from a progression manager")]
        public AbilityUpgradeProgressData UpgradeData { get; protected set; }
        [SerializeField] protected SharedResourceType sharedResource;
        public UIDisplayLocationType UIDisplayLocation;
        [ReadOnly] public float sharedRechargeRemaining;
        [ReadOnly] public float sharedRechargeLastMax;
        [HideInInspector] public int sharedChargeRemaining;
        [HideInInspector] public int sharedChargeMax;
        [HideInInspector] public int castWhileUsingCount;
        [HideInInspector] public bool sharedRechargeHandler; //is this the ability which handles the shared recharge for all abilites on this character?
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

        LightfallAbilityBase waitingForAbilityToFinishForStart;//used to delay this ability from starting when it was attempted to start, but another ability needs to be stopped and wrap up...

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (abilitySO == null)
                Debug.Log($"No ability has been assigned to LightfallAbility on {m_CharacterLocomotion.gameObject}.");

            lightFallAbilitesEquipped = new List<LightfallAbilityBase>();

            sharedRechargeHandler = true;
            foreach (var ability in m_CharacterLocomotion.Abilities)
            {
                LightfallAbilityBase lightfallAbility = ability as LightfallAbilityBase;
                if (lightfallAbility != null)
                {
                    lightFallAbilitesEquipped.Add(lightfallAbility);
                    if (lightfallAbility.sharedRechargeHandler && lightfallAbility != this)
                        sharedRechargeHandler = false;
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
            //NEED TO ADD MAGIC, TECH, COMBAT types/ tags to abilities (or just check that ability "Magic" tag will benifit from increasing damage with "increase damage of MAGIC tag")

            //check if any other abilites are blocking this from starting
            foreach (var ability in lightFallAbilitesEquipped)
            {
                if (ability == this)
                    continue;

                //if another ability is already active...
                if (ability.abilityIsActive)
                {
                    //and cannot be canceled, do not start this ability
                    if (!ability.CanBeCancled)
                        return false;

                    //and this nor the active ability are not equippable...
                    if (!ability.abilityWrapper.IsEquippable && !AbilityWrapper.IsEquippable)
                    {
                        //and we cannot activate more than one active ability at once, do not start this ability (unless this ability is charge based)
                        if (ability.castWhileUsingCount <= 0 && !ability.abilityWrapper.IsChargeBased)
                            return false;
                    }
                }
            }
            //equippable abilities are "abilities", and will only consume a "cast while using" if it is "used", not just equipped.

            return base.CanStartAbility();
        }

        public override bool AbilityWillStart()
        {
            if (abilityWrapper == null)
            {
                Debug.Log($"No abilityWrapper was able to be obtained by LightfallAbility on {m_CharacterLocomotion.gameObject}. Use breakpoints to investigate further.");
                return false;
            }


            //check if ability is sharing cooldowns/charges, and if so check that the shared cooldown/charges allows use...
            if (sharedResource == SharedResourceType.Recharge && sharedRechargeRemaining > 0)
                return false;


            if (sharedResource == SharedResourceType.Charges && sharedChargeRemaining <= 0)
                return false;



            //reduce the castWhileUsingCount of any other abilites
            foreach (var ability in lightFallAbilitesEquipped)
            {
                //If the ability has an extra cast, is not an equippable ability, and is not this ability, then reduce the castWhileUsingCount;
                if (ability.castWhileUsingCount > 0 && !ability.abilityWrapper.IsEquippable && ability != this)
                {
                    castWhileUsingCount--;
                    break;
                }
            }

            //validate that the ability itself has all references and passes all conditions it has internally
            if (!abilityWrapper.ValidateUse())
                return false;

            abilityIsActive = true;

            //cancel any equippable abilites
            foreach (var ability in lightFallAbilitesEquipped)
            {
                if (ability != this && ability.abilityWrapper.IsEquippable && ability.IsActive)
                {
                    ability.StopAbility();
                    waitingForAbilityToFinishForStart = ability;
                    return false;
                }
            }


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

            abilityWrapper.Update();

            //if we were waiting for an ability to finish, check if it has wrapped up
            if (waitingForAbilityToFinishForStart != null)
            {
                if (waitingForAbilityToFinishForStart.AbilityWrapper.AbilityState == AbilityState.Inactive)
                {
                    waitingForAbilityToFinishForStart = null;
                    StartAbility();
                }
            }

            //update the shared recharge of lightfall abilities on this object
            if (sharedRechargeHandler && sharedRechargeRemaining > 0)
            {
                sharedRechargeRemaining -= Time.deltaTime;
                if (sharedRechargeRemaining < 0)
                    sharedRechargeRemaining = 0;

                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedResource == SharedResourceType.Recharge)
                    {
                        ability.sharedRechargeRemaining = sharedRechargeRemaining;
                        ability.sharedRechargeLastMax = sharedRechargeLastMax;
                    }
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
            castWhileUsingCount = 0;

            //update the shared recharges of lightfall abilities on this object
            if (sharedResource == SharedResourceType.Recharge)
            {


                foreach (var ability in lightFallAbilitesEquipped)
                {
                    if (ability.sharedRechargeHandler)
                    {
                        ability.sharedRechargeRemaining += abilityWrapper.RechargeRemaining;
                        ability.sharedRechargeLastMax = ability.sharedRechargeRemaining;
                        sharedRechargeRemaining = ability.sharedRechargeRemaining;
                        sharedRechargeLastMax = ability.sharedRechargeLastMax;
                    }
                    //if (ability.sharedResource == SharedResourceType.Recharge)
                    //ability.sharedRechargeLastMax = sharedRechargeLastMax;
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
