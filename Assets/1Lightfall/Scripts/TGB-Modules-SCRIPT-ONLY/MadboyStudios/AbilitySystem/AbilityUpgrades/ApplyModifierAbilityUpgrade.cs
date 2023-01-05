using MBS.DamageSystem;
using MBS.ModifierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class ApplyModifierAbilityUpgrade : AbilityUpgradeBase
    {
        [SerializeField]
        private ModifierBase modifier;
        [SerializeField]
        private ApplyStratagy applyStratagy;

        private ModifierHandler modifierHandler;

        public override void Use(AbilityWrapperBase wrapperAbility)
        {
            if (modifierHandler == null)
                modifierHandler = wrapperAbility.Origin.GetComponent<ModifierHandler>();

            wrapperAbility.OnUse += InternalUse;
        }

        private void InternalUse(AbilityWrapperBase wrapperAbility)
        {
            switch (applyStratagy)
            {
                case ApplyStratagy.ApplyToSelfOnCast:
                    OnSelf(wrapperAbility);
                    break;
                case ApplyStratagy.ApplyToTargetOnHit:
                    OnTargetOnHit(wrapperAbility);
                    break;
                case ApplyStratagy.ApplyToSelfOnHitTarget:
                    OnSelfOnTargetHit(wrapperAbility);
                    break;
            }

        }

        private void OnSelf(AbilityWrapperBase wrapperAbility)
        {


            ModifierService.Instance.ApplyModifier(wrapperAbility, modifierHandler, modifier);
        }

        private void OnTargetOnHit(AbilityWrapperBase wrapperAbility)
        {
            IDamager wrapperDamager = wrapperAbility;

            wrapperDamager.OnDealDamage += WrapperDamager_OnDealDamage;

            void WrapperDamager_OnDealDamage(IDamageable obj, DamageData damageData)
            {
                ModifierHandler target = obj.gameObject.GetComponent<ModifierHandler>();
                if (target == null)
                    return;

                ModifierService.Instance.ApplyModifier(wrapperAbility, target, modifier);
            }
        }

        private void OnSelfOnTargetHit(AbilityWrapperBase wrapperAbility)
        {
            IDamager wrapperDamager = wrapperAbility;

            wrapperDamager.OnDealDamage += WrapperDamager_OnDealDamage;

            void WrapperDamager_OnDealDamage(IDamageable obj, DamageData damageData)
            {
                ModifierService.Instance.ApplyModifier(wrapperAbility, modifierHandler, modifier);
            }
        }

        public override void GetStats(List<AbilityUIStat> stats, bool hasUpgrade, bool isProspectiveUpgrade)
        {
            //Debug.LogWarning("Trying to get stat from modifier application, but it is unhandled.");
            if (!hasUpgrade && !isProspectiveUpgrade)
                return;

            List<AbilityUIStat> modifierStats = modifier.GetStats();
            if (isProspectiveUpgrade && !hasUpgrade)
            {
                foreach (AbilityUIStat modifierStat in modifierStats)
                {
                    modifierStat.InitalValue = .01f;
                    modifierStat.CurrentValue = .01f;
                }
            }

            stats.AddRange(modifierStats);
        }

    }

    [Serializable]
    public enum ApplyStratagy
    {
        ApplyToSelfOnCast,
        ApplyToTargetOnHit,
        ApplyToSelfOnHitTarget

    }
}



