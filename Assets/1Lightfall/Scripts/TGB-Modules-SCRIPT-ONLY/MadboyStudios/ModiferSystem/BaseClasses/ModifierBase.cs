using MBS.AbilitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [CreateAssetMenu(fileName = "Mod_base new", menuName = "Modifier System/ Create Modifier")]
    public class ModifierBase : ScriptableObject
    {
        [SerializeReference]
        private List<ModifierConditionBase> conditions;
        [SerializeReference]
        private List<ModifierEffectBase> effects;
        public List<IModifierEffect> Effects
        {
            get
            {
                List<IModifierEffect> returnVal = new List<IModifierEffect>();
                for (int i = 0; i < effects.Count; i++)
                {
                    returnVal.Add(effects[i]);
                }
                return returnVal;
            }
        }

        public bool EvaluateConditions(ModifierHandler handlerContext)
        {
            bool passed = false;
            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].Evaluate(handlerContext))
                {
                    if (!conditions[i].ContinueEvaluationIfFalse)
                        break;
                }
                else
                {
                    if (i == conditions.Count - 1)
                        passed = true;
                }
            }

            return passed;
        }



        private void OnValidate()
        {
            foreach (var condition in conditions)
            {
                condition.OnValidate();
            }

            foreach (var effect in effects)
            {
                effect.OnValidate();
            }
        }

        public override string ToString()
        {
            return "";
        }


        public virtual void ApplyAbilitySystemUpgradesToEntries(List<ModifierEntry> entries, AbilityWrapperBase abilityWrapper)
        {
            foreach (var effect in effects)
            {
                //Debug.LogWarning($"you are trying to apply ability system upgrades to {this} from {abilityWrapper.AbilityBase.name}, but it is unhandled.");

                ModifierEntry entryForEffect = entries.Where(entry => entry.Effect == effect).FirstOrDefault();
                if (entryForEffect != null)
                    effect.ApplyAbilitySystemUpgradesToEntries(entryForEffect, abilityWrapper);
            }
        }

        public virtual List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            foreach (var effect in effects)
            {
                returnVal.AddRange(effect.GetStats());
            }


            return returnVal;
        }
    }
}
