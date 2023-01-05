using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class ModifierService : MonoBehaviour
    {

        public static ModifierService Instance;

        //TODO: Have ModifierHandlers add themselves to a dictionary here on enable and remove themselves on disable...
        //in order to easily search for gameobject->ModifierHandler references?

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Applies a modifier if the conditions return true
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="modifier"></param>
        public List<ModifierEntry> ApplyModifier(IOrigin origin, ModifierHandler target, ModifierBase modifier)
        {
            List<ModifierEntry> returnVal = new List<ModifierEntry>();
            if (target == null)
                return returnVal;

            if (!modifier.EvaluateConditions(target))
                return returnVal;

            foreach (var effect in modifier.Effects)
            {
                returnVal.Add(target.AddEntry(origin, effect, modifier.name));
            }

            return returnVal;
        }

        /// <summary>
        /// Applies a modifier if the conditions return true
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="modifier"></param>
        public void ApplyModifier(IOrigin origin, GameObject target, ModifierBase modifier)
        {
            ApplyModifier(origin, target.GetComponentInParent<ModifierHandler>(), modifier);
        }

        ///// <summary>
        ///// Applies an effect directly with no condtion checks 
        ///// </summary>
        ///// <param name="origin"></param>
        ///// <param name="target"></param>
        ///// <param name="effect"></param>
        //public void ApplyEffect(IModifierOrigin origin, ModifierHandler target, ModifierEffectBase effect)
        //{
        //    if (target == null)
        //        return;

        //    target.AddEntry(origin, effect,"");
        //}

        ///// <summary>
        ///// Applies an effect directly with no condtion checks
        ///// </summary>
        ///// <param name="origin"></param>
        ///// <param name="target"></param>
        ///// <param name="effect"></param>
        //public void ApplyEffect(IModifierOrigin origin, GameObject target, ModifierEffectBase effect)
        //{
        //    ApplyEffect(origin, target.GetComponentInParent<ModifierHandler>(), effect);
        //}

    }
}
