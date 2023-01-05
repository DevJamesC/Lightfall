using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.ModifierSystem
{
    public class ConditionBoolean : ModifierConditionBase
    {
        [SerializeField]
        private bool booleanValue;

        public override bool Evaluate(ModifierHandler handlerContext = null)
        {
            return booleanValue;
        }
    }
}
