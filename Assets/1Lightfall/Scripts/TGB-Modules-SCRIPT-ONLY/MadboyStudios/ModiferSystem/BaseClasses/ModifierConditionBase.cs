using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [Serializable]
    public abstract class ModifierConditionBase : IModifierCondition
    {
        [Tooltip("Determines the fail action: continue checking conditions or stop trying to apply modifier")]
        [Title("$subclassName", null, TitleAlignments.Centered), PropertyOrder(-1)]
        public bool ContinueEvaluationIfFalse;

        private string subclassName;//used for GUI
        public virtual bool Evaluate(ModifierHandler handlerContext)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnValidate()
        {
            subclassName = this.GetType().ToString();
        }
    }
}
