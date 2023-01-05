using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityConditionBase
    {
        [Tooltip("Determines the fail condition, such as a recharge time or uses remaining.")]
        [Title("$subclassName", null, TitleAlignments.Centered)]
        [SerializeField, ReadOnly]
        private string subclassName;//used for GUI

        /// <summary>
        /// Validate() is used to check if the condition passes or fails. For traditional Unity Validate() in editor, override OnValidate.
        /// </summary>
        /// <param name="wrapperAbility"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool Validate(AbilityWrapperBase wrapperAbility)
        {

            throw new NotImplementedException("This method needs to be overriden in child classes. Currently it does not know what conditions to check against.");
        }



        public virtual void OnValidate()
        {
            subclassName = this.GetType().ToString();
        }
    }
}

