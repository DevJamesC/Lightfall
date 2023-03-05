using MBS.Lightfall;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Impact;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MBS.DamageSystem;
using Opsive.UltimateCharacterController.Items.Actions.Bindings;
using MBS.ModifierSystem;

namespace MBS.AbilitySystem
{

    //Since this will need to be refactored, here is the code flow:
    //When the upgrade is applied, it gives a delegate to set up a new Impact action when the weapon is instanciated.
    //When this happens, abilityItemHandler.AddThrowableItemImpactModule() is called. This will in turn construct all the impact modules required for the upgrade.
    public enum ImpactModuleUpgrades
    {
        DelayedDamage,
        CauseStagger,
        DetonateMajorEffect
    }

    public class AddImpactActionToAbilityItem : AbilityUpgradeBase
    {
        [SerializeField] protected List<ImpactModuleUpgrades> upgrades = new List<ImpactModuleUpgrades>();

        [SerializeField, ShowIf("@upgrades.Contains(ImpactModuleUpgrades.DelayedDamage)")] protected DelayedDamageInitValues delayedDamageFields;
        [SerializeField, ShowIf("@upgrades.Contains(ImpactModuleUpgrades.CauseStagger)")] protected CauseStaggerInitValues causeStaggerInitFields;
        [SerializeField, ShowIf("@upgrades.Contains(ImpactModuleUpgrades.DetonateMajorEffect)")] protected DetonateMajorEffectInitValues detonateMajorEffectInitFields;

        private AbilityItemHandler abilityItemHandler;
        private bool upgradesAppliedToWeaponItem;
        public override void Use(AbilityWrapperBase wrapperAbility)
        {

            EquipOpsiveItemOnUse equipOpsiveItemOnUse = null;
            foreach (var effect in wrapperAbility.effects)
            {
                equipOpsiveItemOnUse = effect as EquipOpsiveItemOnUse;
                if (equipOpsiveItemOnUse != null)
                    break;
            }

            if (equipOpsiveItemOnUse == null)
                return;

            equipOpsiveItemOnUse.OnSetupAbilityItemHandler += EquipOpsiveItemOnUse_OnSetupAbilityItemHandler;
            upgradesAppliedToWeaponItem = false;
        }

        private void EquipOpsiveItemOnUse_OnSetupAbilityItemHandler(AbilityItemHandler abilityItemHandler)
        {
            if(upgradesAppliedToWeaponItem)
                    return;

            if (this.abilityItemHandler == abilityItemHandler)
                return;
            this.abilityItemHandler = abilityItemHandler;
            upgradesAppliedToWeaponItem = true;

            foreach (var upgrade in upgrades)
            {
                switch (upgrade)
                {
                    case ImpactModuleUpgrades.DelayedDamage:
                        abilityItemHandler.AddThrowableItemImpactModule(upgrade, delayedDamageFields);
                        break;
                    case ImpactModuleUpgrades.CauseStagger:
                        abilityItemHandler.AddThrowableItemImpactModule(upgrade, causeStaggerInitFields);
                        break;
                    case ImpactModuleUpgrades.DetonateMajorEffect:
                        abilityItemHandler.AddThrowableItemImpactModule(upgrade, detonateMajorEffectInitFields);
                        break;
                }

            }

            abilityItemHandler.OnItemDestroyed += () => { upgradesAppliedToWeaponItem = false; };

        }

        public override void GetStats(List<AbilityUIStat> returnVal, bool hasUpgrade, bool isProspectiveUpgrade)
        {

            foreach (var upgrade in upgrades)
            {
                switch (upgrade)
                {
                    case ImpactModuleUpgrades.DelayedDamage:

                        break;
                    case ImpactModuleUpgrades.CauseStagger:

                        break;
                }
            }

            base.GetStats(returnVal, hasUpgrade, isProspectiveUpgrade);
        }
    }

    /// <summary>
    /// Call Initalize to initalize an new impact module to upgrade onto a weapon
    /// </summary>
    [Serializable]
    public class MBSThrowableImpactModule : ThrowableImpactModule
    {
        [Tooltip("The impact actions to invoked on impact.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActions;

        public ImpactActionGroup ImpactActions { get => m_ImpactActions; set => m_ImpactActions = value; }


        public void Initialize(ImpactModuleUpgrades module, CharacterItemAction characterItemAction, MBSThrowableImpactModuleInitValuesBase initValues)
        {
            base.InitializeInternal();

            switch (module)
            {
                case ImpactModuleUpgrades.DelayedDamage:
                    m_ImpactActions = new ImpactActionGroup(new ImpactAction[]
                    {
                     new DelayedDamage(true,initValues as DelayedDamageInitValues),
                     new SpawnSurfaceEffect(true),
                     new ImpactEvent(),
                    });
                    break;
                case ImpactModuleUpgrades.CauseStagger:
                    m_ImpactActions = new ImpactActionGroup(new ImpactAction[]
                   {
                     new CauseStagger(true,initValues as CauseStaggerInitValues),
                   });
                    break;
                case ImpactModuleUpgrades.DetonateMajorEffect:
                    m_ImpactActions = new ImpactActionGroup(new ImpactAction[]
                   {
                     new DetonateMajorEffectImpactAction(true,initValues as DetonateMajorEffectInitValues),
                   });
                    break;
            }
            m_CharacterItemAction = characterItemAction;
            //m_Bindings = new StateObjectBindingGroup();
            //m_Bindings.Initialize(this, Character);
            m_ImpactActions.Initialize(this);
        }


        /// <summary>
        /// Function called when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public override void OnImpact(ImpactCallbackContext impactCallbackContext)
        {
            m_ImpactActions.OnImpact(impactCallbackContext, true);
        }

        /// <summary>
        /// Reset the impact with the source id.
        /// </summary>
        /// <param name="sourceID">The source id of the impact to reset.</param>
        public override void Reset(uint sourceID)
        {
            m_ImpactActions.Reset(sourceID);
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_ImpactActions.OnDestroy();
        }

        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_ImpactActions == null || m_ImpactActions.ImpactActions == null)
            {
                return base.ToString();
            }
            return GetToStringPrefix() + $"Generic ({m_ImpactActions.Count}): " + ListUtility.ToStringDeep(m_ImpactActions.ImpactActions, true);

        }
    }

    [Serializable]
    public class MBSThrowableImpactModuleInitValuesBase
    {
        public float Delay;
    }
    [Serializable]
    public class DelayedDamageInitValues : MBSThrowableImpactModuleInitValuesBase
    {
        public MBSDamageForInspector Damagefields;
    }
    [Serializable]
    public class CauseStaggerInitValues : MBSThrowableImpactModuleInitValuesBase
    {
        public float StaggerChance;
    }
    [Serializable]
    public class DetonateMajorEffectInitValues : MBSThrowableImpactModuleInitValuesBase
    {
        public MajorEffects MajorEffect;
    }

    /// <summary>
    /// An impact module used to deal damage and forces to the target.
    /// </summary>
    [Serializable]
    public class DelayedDamage : LightfallDamage
    {

        //[SerializeField] protected float m_DelayBeforeImpact;

        // public float DelayBeforeImpact { get => m_DelayBeforeImpact; set => m_DelayBeforeImpact = value; }

        public DelayedDamage(bool useContextData) { damageFields.UseContextData = useContextData; }
        public DelayedDamage(bool useContextData, DelayedDamageInitValues initValues)
        {
            damageFields = initValues.Damagefields;
            Delay = initValues.Delay;
            damageFields.UseContextData = useContextData;
        }

        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            Debug.Log("Grenade Shock Damage on " + ctx.ImpactCollisionData.ImpactGameObject.name);
            base.OnImpactInternal(ctx);

        }
    }

    [Serializable]
    public class CauseStagger : ImpactAction
    {
        protected float staggerChance;
        public CauseStagger(bool useContextData, CauseStaggerInitValues initValues)
        {
            staggerChance = initValues.StaggerChance;
            Delay = initValues.Delay;
        }
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            if (ctx == null)
                return;
            if (ctx.ImpactCollisionData == null)
                return;
            if (ctx.ImpactCollisionData.ImpactGameObject == null)
                return;

            //NOTE: when there is a delay, the context is cached and overwritten... so all delayed actions are using the "last hit" context, rather than the context associated with them
            //Debug.Log("Grenade staggering "+ctx.ImpactCollisionData.ImpactGameObject.name);

            LightfallHealth targetHealth = ctx.ImpactCollisionData.ImpactGameObject.GetComponent<LightfallHealth>();
            if (targetHealth == null)
                return;

            targetHealth.TryStaggerRawPercent(staggerChance);
        }
    }
    [Serializable]
    public class DetonateMajorEffectImpactAction : ImpactAction
    {
        protected MajorEffects majorEffect;
        public DetonateMajorEffectImpactAction(bool useContextData, DetonateMajorEffectInitValues initValues)
        {
            majorEffect = initValues.MajorEffect;
        }
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            Debug.LogWarning("Not Implimented");
        }
    }
}
