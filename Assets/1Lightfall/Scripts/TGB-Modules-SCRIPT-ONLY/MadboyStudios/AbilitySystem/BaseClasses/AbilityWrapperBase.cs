using MBS.DamageSystem;
using MBS.ForceSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Items.Actions.Impact;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityWrapperBase : IDamager, IForcer, IOrigin
    {
        public bool AbilitySupressed { get; protected set; }
        public GameObject Origin { get; protected set; }
        public List<Tag> OriginTags { get; set; }
        public AbilityState AbilityState { get; protected set; }
        public AbilityUpgradeProgressData UpgradeData { get; protected set; }

        public AbilityBase AbilityBase { get; protected set; }
        protected List<AbilityUpgradeBase> activeUpgrades;
        protected List<AbilityConditionBase> conditions;
        internal List<AbilityEffectBase> effects;
        protected List<AbilityFXBase> fX;

        public event Action<AbilityWrapperBase> OnUse = delegate { };
        public event Action<int> OnChargesChanged = delegate { };
        public event Action<AbilityWrapperBase> OnUpdate = delegate { };
        public event Action<AbilityWrapperBase> OnFinishUse = delegate { };
        public event Action<AbilityWrapperBase> OnCanceled = delegate { };
        public event Action<AbilityWrapperBase> OnDisposed = delegate { };

        public event Action<IDamageable, DamageData> OnDealDamage = delegate { };
        public event Action<IDamageable, Vector3, Collider> OnDealDamageWithCollisionData = delegate { };

        private Dictionary<StatName, DynamicStatModifierForAbilites> LocalStatModifiersFromUpgrades;

        public List<Action<DamageData, IDamageable>> OnPreDamageDelegatesForCleanup;

        private bool abilityUnlocked;



        private float timeTillUse;

        private float effectsWaitingToFinish;

        public ModifierHandler ModifierHandler;

        public float InitalRecharge { get; set; }//set by recharge script and read by UI script to deduce percent cooldown remaining
        public float RechargeRemaining { get; set; }

        public int MaxCharges { get; set; }
        public int BaseMaxCharges { get; set; }
        public int ChargesRemaining { get; protected set; }

        public bool CanBeCanceled
        {
            get
            {
                bool returnVal = true;
                foreach (var effect in effects)
                {
                    if (!effect.CanBeCanceled)
                    {
                        return false;
                    }
                }
                return returnVal;
            }
        }

        public void ChangeChargesRemaining(int changeVal)
        {
            ChargesRemaining += changeVal;
            if (ChargesRemaining < 0)
                ChargesRemaining = 0;

            OnChargesChanged.Invoke(ChargesRemaining);
        }
        public void SetChargesRemaining(int setVal)
        {
            ChargesRemaining = setVal;
            if (ChargesRemaining < 0)
                ChargesRemaining = 0;
        }

        public void AddAbilityEffect(AbilityEffectBase effect, bool addEffectToEffectsList = true)
        {
            AbilityEffectBase newEffect = effect;
            if (addEffectToEffectsList)
            {
                newEffect = effect.GetShallowCopy();
                effects.Add(newEffect);
            }

            newEffect.Init(this);
            OnUpdate += newEffect.OnUpdate;
            newEffect.OnEffectFinished += () => { effectsWaitingToFinish--; };
        }

        public bool CancelableOnOtherAbilityCast { get; set; } //Set by EquipOnUseAbilityEffect and read by AbilityLoadout.

        /// <summary>
        /// Returns Origin. Used for interface.
        /// </summary>
        public GameObject gameObject => Origin;

        public float Damage { get; set; }
        public DamageSourceType DamageSourceType { get; set; }
        public int Force { get; set; }

        public AbilityWrapperBase(AbilityBase abilityBase, AbilityUpgradeProgressData upgradeData, GameObject origin, ModifierHandler originHandler, TagHandler originTags)
        {
            if (abilityBase == null)
                return;

            Origin = origin;
            AbilityBase = abilityBase;
            abilityUnlocked = upgradeData.AbilityUnlocked;
            UpgradeData = upgradeData;
            conditions = abilityBase.AbilityConditions;
            ModifierHandler = originHandler == null ? origin.GetComponent<ModifierHandler>() : originHandler;
            DamageSourceType = DamageSourceType.AbilityDamage;
            OriginTags = originTags == null ? origin.GetComponent<TagHandler>().Tags : OriginTags;
            activeUpgrades = new List<AbilityUpgradeBase>();
            OnPreDamageDelegatesForCleanup = new List<Action<DamageData, IDamageable>>();
            effects = new List<AbilityEffectBase>();
            fX = new List<AbilityFXBase>();
            CancelableOnOtherAbilityCast = false;

            //Get shallow copy of upgrades from SO
            foreach (var item in abilityBase.GetActiveUpgrades(upgradeData))
            {
                activeUpgrades.Add(item.GetShallowCopy());
            }
            //Get shallow copy of effect from SO
            foreach (var item in abilityBase.AbilityEffects)
            {
                effects.Add(item.GetShallowCopy());
            }
            //Get shallow copy of FX from SO
            foreach (var item in abilityBase.OnActivateFX)
            {
                fX.Add(item.GetShallowCopy());
            }

            //setup all effects attached to this ability
            foreach (var effect in effects)
            {
                AddAbilityEffect(effect, false);

            }

            //setup all FX attached to this ability
            foreach (var fx in fX)
            {
                OnUpdate += fx.OnUpdate;
            }

            //apply all upgrades 
            foreach (var upgrade in activeUpgrades)
            {
                upgrade.Use(this);
            }

            //Handle setting up ability Charges
            ValidateStatModifier(StatName.AbilityCapacity);
            MaxCharges = Mathf.FloorToInt(BaseMaxCharges + (BaseMaxCharges * LocalStatModifiersFromUpgrades[StatName.AbilityCapacity].PercentValue) + LocalStatModifiersFromUpgrades[StatName.AbilityCapacity].FlatValue);
            ChargesRemaining = MaxCharges;

            if (abilityBase.AbilityType != AbilityType.Passive)//Set inital ability state
                AbilityState = AbilityState.Inactive;
            else if (abilityBase.AbilityType == AbilityType.Passive)//apply passives immedietly
            {
                AbilityState = AbilityState.InUseInBackground;
                if (upgradeData.AbilityUnlocked)
                    Use();
            }
        }

        public bool TryUse()//sets up the timeTillUse and abilityState based on the ability settings
        {
            //Validate that this ability has all the references it needs (original ability, upgradeProgress, and the ability is unlocked) and passes all conditions
            if (!ValidateUse())
                return false;

            //If the ability is in use, then just call UseWhileInUse()
            if (AbilityState == AbilityState.InUse || AbilityState == AbilityState.InUseInBackground)
            {
                //Start deactivating OnOff abilites. 
                if (AbilityBase.AbilityType == AbilityType.OnOffActivatable)
                    AbilityState = AbilityState.Deactivating;

                foreach (var effect in effects)
                {
                    effect.UseWhileInUse(this);
                }
                return false;
            }

            //Passive abilites cannot be "used", and abilites must be inactive before they are used.
            if (AbilityBase.AbilityType == AbilityType.Passive || AbilityState != AbilityState.Inactive)
                return false;


            //Start up ability
            AbilityState = AbilityState.StartingUp;

            foreach (var fx in fX)
            {
                fx.Activate(this);
            }
            timeTillUse = AbilityBase.TimeToUse;

            return true;
        }

        private void Use()
        {
            effectsWaitingToFinish = effects.Count;


            foreach (AbilityEffectBase abilityEffect in effects)
            {
                abilityEffect.Use(this);
            }

            OnUse.Invoke(this);

            if (AbilityState != AbilityState.Deactivating && AbilityBase.AbilityType != AbilityType.Passive)
                AbilityState = AbilityState.InUse;
        }

        private void FinishAbility()//This is called after "effectsWaitingToFinish" equals 0
        {
            if (AbilityBase.AbilityType == AbilityType.StandardActivatable)
                AbilityState = AbilityState.Inactive;

            if (AbilityBase.AbilityType == AbilityType.OnOffActivatable)
            {
                if (AbilityState == AbilityState.InUse)
                    AbilityState = AbilityState.InUseInBackground;
                else if (AbilityState == AbilityState.Deactivating)
                    AbilityState = AbilityState.Inactive;
            }


            if (AbilityState == AbilityState.Inactive)
            {
                foreach (var fx in fX)
                {
                    fx.Deactivate(this);
                }

                OnFinishUse.Invoke(this);
            }
        }

        public virtual bool ValidateUse()
        {
            if (AbilityBase == null)
                return false;
            if (activeUpgrades == null)
                return false;
            if (!abilityUnlocked)
                return false;
            if (AbilitySupressed)
                return false;

            foreach (var condition in conditions)
            {
                if (!condition.Validate(this))
                    return false;
            }

            return true;
        }

        public void Update()
        {
            if (timeTillUse > 0)
                timeTillUse -= Time.deltaTime;

            if ((AbilityState == AbilityState.StartingUp || AbilityState == AbilityState.Deactivating) && timeTillUse <= 0 && effectsWaitingToFinish <= 0)
            {
                Use();
            }

            OnUpdate.Invoke(this);

            if ((AbilityState == AbilityState.InUse || AbilityState == AbilityState.Deactivating) && effectsWaitingToFinish == 0)
                FinishAbility();
        }

        public void DealDamage(IDamageable damageableHit, Vector3 hitPoint, Collider colliderHit = null)
        {
            OnDealDamage.Invoke(damageableHit, null);
            OnDealDamageWithCollisionData.Invoke(damageableHit, hitPoint, colliderHit);
        }

        /// <summary>
        /// Use this whenever you no longer wish to reference the ability wrapper. 
        /// All this does is un-apply any passive or onOff abilites that may be on the entity.
        /// </summary>
        public void DisposeAbilityWrapper()
        {

            foreach (var fx in fX)
            {
                fx.Deactivate(this);
            }

            foreach (var effect in effects)
            {
                effect.Dispose(this);
            }

            foreach (var item in OnPreDamageDelegatesForCleanup)
            {
                ModifierHandler.OnPreDamageEffects -= item;
            }
            OnDisposed.Invoke(this);
        }

        /// <summary>
        /// Returns the value of the local ability upgrade modifiers plus any changes from the Modifier System.
        /// Recharge speed is handled as a special case, and is NormalModifier=True (though it'll handle it if false as well).
        /// </summary>
        /// <param name="statName">Name of the stat to query</param>
        /// <param name="baseValue">Base value to pass in</param>
        /// <param name="normalModifier">Should the modifer be applied to the base value as a normal value, or a hard value?</param>
        /// <param name="includeLocalStatChanges">Should the return value include local stat changes, or just ModifierHandler changes?</param>
        /// <returns></returns>
        public float GetStatChange(StatName statName, float baseValue, bool normalModifier, bool includeAbilityLocalStatChanges = true, bool includeModifierStatChanges = true)
        {
            float returnVal = baseValue;
            if (statName == StatName.AbilityRecharge)
            {
                returnVal = AbilityModifierFormulas.RechargeSpeedModifier(baseValue,
                    (includeAbilityLocalStatChanges ? GetLocalStatModifierValue(statName, true) : 0) +
                    (includeModifierStatChanges ? ModifierHandler.GetStatModifierValue(statName) : 0));
                returnVal += GetLocalStatModifierValue(statName, false);
                return returnVal;
            }

            float modifierHandlerValue = includeModifierStatChanges ? ModifierHandler.GetStatModifierValue(statName) : 0;
            if (!normalModifier && includeModifierStatChanges)
                modifierHandlerValue -= 1;

            if (includeAbilityLocalStatChanges)
            {
                if (normalModifier)
                    returnVal = AbilityModifierFormulas.NormalModifier(baseValue, GetLocalStatModifierValue(statName, true) + modifierHandlerValue);
                else
                    returnVal = AbilityModifierFormulas.HardValueModifier(baseValue, GetLocalStatModifierValue(statName, false) + modifierHandlerValue);
            }



            return returnVal;
        }

        /// <summary>
        /// This returns the value of an ability stat modifier.
        /// If PercentValueIncrease is true, the value is applied to a percent increase. If false, value is applied to flat increase.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private float GetLocalStatModifierValue(StatName stat, bool PercentValueIncrease)
        {
            ValidateStatModifier(stat);
            if (PercentValueIncrease)
                return LocalStatModifiersFromUpgrades[stat].PercentValue;
            else
                return LocalStatModifiersFromUpgrades[stat].FlatValue;
        }
        /// <summary>
        /// This adds or subtracts from the current ability stat modifier. This does NOT "Set" the modifier value!
        /// If PercentValueIncrease is true, it returns the percent change. If false, it returns the flat value change, if any.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void ChangeStatModifierValue(StatName stat, float value, bool PercentValueIncrease)
        {
            ValidateStatModifier(stat);
            if (PercentValueIncrease)
                LocalStatModifiersFromUpgrades[stat].PercentValue += value;
            else
                LocalStatModifiersFromUpgrades[stat].FlatValue += value;

            if (stat == StatName.AbilityCapacity)
            {
                MaxCharges = Mathf.FloorToInt(BaseMaxCharges + (BaseMaxCharges * LocalStatModifiersFromUpgrades[StatName.AbilityCapacity].PercentValue) + LocalStatModifiersFromUpgrades[StatName.AbilityCapacity].FlatValue);
            }

        }

        /// <summary>
        /// This makes sure that the ability stat which is being modified actually has an entry in the dictonary. Lazy instanciation.
        /// </summary>
        /// <param name="stat"></param>
        private void ValidateStatModifier(StatName stat)
        {
            if (LocalStatModifiersFromUpgrades == null)
                LocalStatModifiersFromUpgrades = new Dictionary<StatName, DynamicStatModifierForAbilites>();

            if (LocalStatModifiersFromUpgrades.ContainsKey(stat))
                return;

            LocalStatModifiersFromUpgrades.Add(stat, new DynamicStatModifierForAbilites(stat));
        }

        /// <summary>
        /// Used by EquippableAbilityBase to cancel using an equipment that has not been consumed yet.
        /// </summary>
        /// <param name="invokeOnAbilityFinished">should we invoke the ability finished action? If we want cooldowns to trigger, then true, otherwise, false</param>
        public void CancelAbility(bool invokeOnAbilityFinished)
        {
            if (AbilityBase.AbilityType == AbilityType.Passive)
                return;

            foreach (var effect in effects)
            {
                effect.CancelEffect();
            }

            AbilityState = AbilityState.Inactive;

            foreach (var fx in fX)
            {
                fx.Deactivate(this);
            }

            if (invokeOnAbilityFinished)
                OnFinishUse.Invoke(this);

            OnCanceled.Invoke(this);

        }

        public void SetOrigin(GameObject newOrigin)
        {
            Origin = newOrigin;
        }

        public class DynamicStatModifierForAbilites
        {
            public StatName Name { get; private set; }
            public float PercentValue { get; set; }
            public float FlatValue { get; set; }

            public DynamicStatModifierForAbilites(StatName stat)
            {
                PercentValue = 0;
                FlatValue = 0;
            }
        }
    }
}



