using MBS.AbilitySystem;
using MBS.DamageSystem;
using MBS.StatsAndTags;
using Opsive.Shared.Events;
using Opsive.Shared.Utility;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Impact;
using Opsive.UltimateCharacterController.Items.Actions.Modules;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.Traits.Damage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.Lightfall
{
    public class AbilityItemHandler : MonoBehaviour
    {

        public event Action OnItemDestroyed = delegate { };
        public float PercentUseRemaining { get => currentCharges / maxCharges; }
        public Action OnFinishUse = delegate { };
        public Action OnUse = delegate { };

        private int maxCharges;
        private int currentCharges;

        private ThrowableAction throwableAction;
        private ShootableAction shootableAction;
        private Action shootableActionDelegates = delegate { };

        private AbilityWrapperBase sourceAbilityWrapper;

        private bool delegatesAppliedFlag;

        public bool IsInUse
        {
            get
            {
                if (throwableAction != null)
                {
                    if (throwableAction.IsItemInUse())
                        return true;
                    if (Time.time - throwableAction.LastUseTime < .05f)
                        return true;

                    return false;
                }

                if (shootableAction != null)
                    return shootableAction.IsItemInUse();


                return false;
            }
        }

        private void Awake()
        {
            delegatesAppliedFlag = false;
        }

        public void Initalize(AbilityWrapperBase abilityWrapper)
        {
            bool ableToFindComponentsToSetup = false;
            sourceAbilityWrapper = abilityWrapper;
            SetCharges();

            if (SetupThrowableItem())
                ableToFindComponentsToSetup = true;
            else if (SetupShootableItem())
                ableToFindComponentsToSetup = true;

            if (!ableToFindComponentsToSetup)
                OnFinishUse.Invoke();

            delegatesAppliedFlag = true;
        }

        private void SetCharges()
        {
            if (sourceAbilityWrapper.MaxCharges > 0)
            {
                maxCharges = sourceAbilityWrapper.ChargesRemaining;
                currentCharges = sourceAbilityWrapper.ChargesRemaining;
            }
            else
            {
                maxCharges = 1;
                currentCharges = 1;
            }



        }

        private bool SetupThrowableItem()
        {
            throwableAction = GetComponent<ThrowableAction>();
            if (throwableAction == null)
                return false;

            if (!delegatesAppliedFlag)
            {
                throwableAction.OnThrowE += ThrowableAction_OnThrowE;
            }


            int chargesDiff = maxCharges - throwableAction.RemainingAmmoCount;
            throwableAction.MainAmmoModule.AdjustAmmoAmount(chargesDiff);

            if (throwableAction.RemainingAmmoCount <= 0)
                return false;

            return true;
        }

        private bool SetupShootableItem()
        {
            shootableAction = GetComponent<ShootableAction>();
            if (shootableAction == null)
                return false;

            if (!delegatesAppliedFlag)
            {
                Opsive.Shared.Events.EventHandler.RegisterEvent(shootableAction.gameObject, "OnItemStartUse", shootableActionDelegates);
                shootableActionDelegates += ShootableAction_OnShoot;
                Debug.Log("delegates assigned to shootable ability. you should see more debug logs come up if it is working. remove these once working is confirmed.");
            }

            return true;
        }

        private void ThrowableAction_OnThrowE(Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable.ThrowableThrowData obj)
        {
            currentCharges--;
            OnUse.Invoke();


            AbilityProjectileHandler abilityProjectileHandler = obj.ProjectileData.m_SpawnedProjectile.GetComponent<AbilityProjectileHandler>();
            if (abilityProjectileHandler == null)
                abilityProjectileHandler = obj.ProjectileData.m_SpawnedProjectile.AddComponent<AbilityProjectileHandler>();
            abilityProjectileHandler.Initalize(sourceAbilityWrapper);
            MBSThrowableImpactModule module = throwableAction.ImpactModuleGroup.Modules.Count > 0 ? throwableAction.ImpactModuleGroup.Modules[0] as MBSThrowableImpactModule : null;
            if (module != null)
            {
                abilityProjectileHandler.AddImpactActions(module.ImpactActions.ImpactActions);
            }

            if (currentCharges <= 0)
            {
                OnFinishUse.Invoke();
                OnFinishUse = delegate { };
                OnUse = delegate { };
            }
        }

        private void ShootableAction_OnShoot()
        {
            currentCharges--;
            OnUse.Invoke();
            Debug.Log("ability shot");

            //Since Opsive removed ShootableProjectileData from FireData, this code needs to be refactored (also it was never tested because we had no "shoot" abilities)
            //if (shootableAction.ShootableUseDataStream.FireData ShootableProjectileData.SpawnedProjectileGO != null)
            //{
            //    AbilityProjectileHandler abilityProjectileHandler = shootableAction.ShootableUseDataStream.ShootableProjectileData.SpawnedProjectileGO.GetComponent<AbilityProjectileHandler>();
            //    if (abilityProjectileHandler == null)
            //        abilityProjectileHandler = shootableAction.ShootableUseDataStream.ShootableProjectileData.SpawnedProjectileGO.AddComponent<AbilityProjectileHandler>();
            //    abilityProjectileHandler.Initalize(sourceAbilityWrapper);
            //}

            //if (currentCharges <= 0)
            //{
            //    OnFinishUse.Invoke();
            //    OnFinishUse = delegate { };
            //    OnUse = delegate { };
            //}
        }

        public void AddThrowableItemImpactModule(ImpactModuleUpgrades upgrade, MBSThrowableImpactModuleInitValuesBase initValues)
        {
            if (throwableAction != null)
            {
                MBSThrowableImpactModule currentMod = null;
                if (throwableAction.ImpactModuleGroup.Modules.Count > 0)
                    currentMod = throwableAction.ImpactModuleGroup.Modules[0] as MBSThrowableImpactModule;
                List<ImpactAction> currentActions = new List<ImpactAction>();
                if (currentMod != null)
                {
                    currentActions.AddRange(currentMod.ImpactActions.ImpactActions);
                }
                //create new moduleGroup and module
                throwableAction.ImpactModuleGroup = new ActionModuleGroup<ThrowableImpactModule>();
                MBSThrowableImpactModule newModule = new MBSThrowableImpactModule();
                //Add the module to the moduleGroup and initalize
                throwableAction.ImpactModuleGroup.AddModule(newModule, throwableAction.gameObject);
                newModule.Initialize(upgrade, throwableAction, initValues);
                //make sure current/ previous actions persist, rather than being overridden
                currentActions.InsertRange(0, newModule.ImpactActions.ImpactActions);
                newModule.ImpactActions.ImpactActions = currentActions.ToArray();

                Debug.Log("action added");
            }
        }

        private void OnDestroy()
        {
            OnItemDestroyed.Invoke();
        }

    }



    public class AbilityProjectileHandler : MonoBehaviour
    {
        private AbilityWrapperBase sourceAbilityWrapper;
        private MBSExplosion explosion;

        private bool delegatesAppliedFlag;
        private bool secondaryDelegatesAppliedFlag;
        private List<ImpactAction> impactActionsFromUpgrades;

        private void Awake()
        {
            delegatesAppliedFlag = false;
            secondaryDelegatesAppliedFlag = false;
        }
        public void Initalize(AbilityWrapperBase abilityWrapper)
        {
            sourceAbilityWrapper = abilityWrapper;
            bool ableToFindComponentsToSetup = false;
            if (impactActionsFromUpgrades == null)
                impactActionsFromUpgrades = new List<ImpactAction>();

            if (SetupGrenadeItem())
                ableToFindComponentsToSetup = true;
            else if (SetupExplosion(gameObject))
                ableToFindComponentsToSetup = true;

            if (!ableToFindComponentsToSetup)
                Debug.LogWarning($"{gameObject.name} is a projectile from an ability, but no component on the projectile can adjust stats based on AbilityWrapper.");

            delegatesAppliedFlag = true;
        }

        private bool SetupGrenadeItem()
        {
            MBSGrenade grenade = GetComponent<MBSGrenade>();
            if (grenade == null)
                return false;

            if (!delegatesAppliedFlag)
                grenade.OnSpawnDestructionObject += (spawnedObject) =>
                {
                    SetupExplosion(spawnedObject);
                };


            return true;
        }

        private bool SetupExplosion(GameObject sourceObject)
        {
            explosion = sourceObject.GetComponent<MBSExplosion>();
            if (explosion == null)
                return false;

            explosion.ResetExplosionValues();

            //adjust values based on stat changes
            explosion.Radius = sourceAbilityWrapper.GetStatChange(StatName.AbilityRadius, explosion.Radius, true);
            explosion.ImpactDamageData.DamageAmount = sourceAbilityWrapper.GetStatChange(StatName.AbilityDamage, explosion.ImpactDamageData.DamageAmount, true);
            explosion.ImpactDamageData.ImpactForce = sourceAbilityWrapper.GetStatChange(StatName.AbilityForce, explosion.ImpactDamageData.ImpactForce, true);
            MBSExtraDamageData extraDamageFields = explosion.ImpactDamageData.GetUserData<MBSExtraDamageData>();
            extraDamageFields.StaggerForce = sourceAbilityWrapper.GetStatChange(StatName.AbilityForce, extraDamageFields.StaggerForce, true);
            extraDamageFields.ShieldEffectiveness = sourceAbilityWrapper.GetStatChange(StatName.AbilityShieldEffectiveness, extraDamageFields.ShieldEffectiveness, false);
            extraDamageFields.ArmorEffectiveness = sourceAbilityWrapper.GetStatChange(StatName.AbilityArmorEffectiveness, extraDamageFields.ArmorEffectiveness, false);
            //add actions from upgrades 
            if (!secondaryDelegatesAppliedFlag)
            {
                List<ImpactAction> newImpactActions = new List<ImpactAction>();
                newImpactActions.AddRange(explosion.ImpactActionGroup.ImpactActions);
                newImpactActions.AddRange(impactActionsFromUpgrades);
                explosion.ImpactActionGroup.ImpactActions = newImpactActions.ToArray();
                secondaryDelegatesAppliedFlag = true;
            }

            return true;
        }

        public void AddImpactActions(ImpactAction[] actions)
        {
            if (impactActionsFromUpgrades == null)
                impactActionsFromUpgrades = new List<ImpactAction>();

            impactActionsFromUpgrades.AddRange(actions);
        }


    }
}