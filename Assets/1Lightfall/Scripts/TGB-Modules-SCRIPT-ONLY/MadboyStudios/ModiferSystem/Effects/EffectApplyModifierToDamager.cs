using MBS.DamageSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class EffectApplyModifierToDamager : ModifierEffectBase
    {

        [SerializeField]
        private ModifierBase modifierToApply;
        [SerializeField]
        private bool ChanceToApplyBasedOnDamage;
        [SerializeField, Range(0, 100), ShowIf("@ChanceToApplyBasedOnDamage==false")]
        private float ChanceToApply;
        [SerializeField, ShowIf("@ChanceToApplyBasedOnDamage==true")]
        private AnimationCurve ChanceToApplyAnimCurve = AnimationCurve.Linear(0, 0, 600, 100);

        public event Action OnEffectApplied = delegate { };
        [SerializeReference, Tooltip("Any FX to apply when the damager successfully applied the affect to a target. Useful for feedbacks, such as 'you poisoned someone!'. " +
                                    "\nTHESE DO NOT AUTO-DEACTIVATE. Therefore they should be one-off FX or otherwise disable themselves.")]
        [BoxGroup("Lists")]
        private List<ModifierFXBase> onEffectAppliedFX = new List<ModifierFXBase>();
        [SerializeReference]
        private EffectAlreadyExistsApplyModifierToDamagerImplimentation effectAlreadyExistsLogic;

        public override void EffectActivated(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);

            //check if the target already has this effect on them. If so, determine what action to take
            if (effectAlreadyExistsLogic.HandleEffectAlreadyExists(targetEntry, targetEntry.Target.GetModifierEntries(targetEntry.Effect)))
                return;

            //get damager
            IDamager targetDamager = targetEntry.Target.gameObject.GetComponentInChildren<IDamager>();
            if (targetDamager == null)
                return;


            ApplyModifierToDamagerEffectStateData newData = new ApplyModifierToDamagerEffectStateData(targetDamager,
                (damageable, damageData) => { OnDamagerHit(targetEntry, damageable, damageData); });

            targetEntry.EffectStateData = newData;

            newData.TargetDamager.OnDealDamage += newData.methodOnDealDamage;


        }

        private void OnDamagerHit(ModifierEntry targetEntry, IDamageable damageable, DamageData damageData)
        {
            //calculate chance to apply
            float percent = ChanceToApplyBasedOnDamage ? ChanceToApplyAnimCurve.Evaluate(damageData.Damage) : ChanceToApply;

            if (ChanceToApply < 100)
            {
                float result = UnityEngine.Random.Range(0, 100);
                //result must be lower than percent to succeed
                Debug.Log($"Rolled {result}, needed {percent} or lower");
                if (result > percent)
                    return;
            }


            ModifierService.Instance.ApplyModifier(targetEntry.Origin, damageable.gameObject, modifierToApply);
            for (int i = 0; i < onEffectAppliedFX.Count; i++)
            {
                onEffectAppliedFX[i].Activate(targetEntry);
            }

            OnEffectApplied.Invoke();
        }

        public override void EffectUpdate(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);
            //hook into FX?
        }

        public override void EffectRemoved(ModifierEntry targetEntry)
        {
            base.EffectRemoved(targetEntry);
            ApplyModifierToDamagerEffectStateData data = targetEntry.EffectStateData as ApplyModifierToDamagerEffectStateData;
            data.TargetDamager.OnDealDamage -= data.methodOnDealDamage;
        }

        public override void OnValidate()
        {
            base.OnValidate();
            effectAlreadyExistsLogic.PopulateTags(tags);
        }
    }

    public class ApplyModifierToDamagerEffectStateData : EffectStateData
    {
        public IDamager TargetDamager;
        public Action<IDamageable, DamageData> methodOnDealDamage;

        public ApplyModifierToDamagerEffectStateData(IDamager targetDamager, Action<IDamageable, DamageData> methodOnDealDamage)
        {
            TargetDamager = targetDamager;
            this.methodOnDealDamage = methodOnDealDamage;
        }
    }
}
