using MBS.ModifierSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class ApplyModifierEffectOnAbilityHitAbilityEffect : AbilityEffectBase
    {
        [SerializeField]
        private ModifierBase applyEffectToTargetOnHit;
        [SerializeField]
        private ApplyStratagy applyStratagy = ApplyStratagy.ApplyToTargetOnHit;
        private ModifierHandler selfModifierHandler;

        protected override void OnStart(AbilityWrapperBase abilityWrapper)
        {
            base.OnStart(abilityWrapper);
            selfModifierHandler = abilityWrapper.Origin.GetComponent<ModifierHandler>();

            if (applyStratagy != ApplyStratagy.ApplyToSelfOnCast)
                abilityWrapper.OnDealDamage += (damagable, damageData) =>
                {
                    ModifierHandler modifierHandler = null;

                    switch (applyStratagy)
                    {
                        case ApplyStratagy.ApplyToSelfOnHitTarget:
                            modifierHandler = selfModifierHandler;
                            break;
                        case ApplyStratagy.ApplyToTargetOnHit:
                            modifierHandler = damagable.gameObject.GetComponent<ModifierHandler>();
                            break;
                    }

                    if (modifierHandler == null)
                        return;

                    List<ModifierEntry> entries = ModifierService.Instance.ApplyModifier(abilityWrapper, modifierHandler, applyEffectToTargetOnHit);
                //apply ability local upgrades to effect
                applyEffectToTargetOnHit.ApplyAbilitySystemUpgradesToEntries(entries, abilityWrapper);



                };
            else
                abilityWrapper.OnUse += (abilityWrapper) =>
                {
                    List<ModifierEntry> entries = ModifierService.Instance.ApplyModifier(abilityWrapper, selfModifierHandler, applyEffectToTargetOnHit);
                //apply ability local upgrades to effect
                applyEffectToTargetOnHit.ApplyAbilitySystemUpgradesToEntries(entries, abilityWrapper);
                };
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.AddRange(applyEffectToTargetOnHit.GetStats());

            return returnVal;
        }
    }
}
