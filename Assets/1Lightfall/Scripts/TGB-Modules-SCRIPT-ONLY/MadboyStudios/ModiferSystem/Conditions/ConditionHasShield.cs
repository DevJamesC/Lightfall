using MBS.HealthSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.ModifierSystem
{
    public class ConditionHasShield : ModifierConditionBase
    {
        [SerializeField, Tooltip("If set True, the condition will pass if the target has a shield with a value greater than 0. " +
                                "If set false, the condition will pass if the target has no health or shield component, or a sheild with value of 0.")]
        private bool PassIfShieldUp;
        public override bool Evaluate(ModifierHandler handlerContext)
        {
            Health healthComp = handlerContext.gameObject.GetComponent<Health>();
            if (healthComp == null)
                return !PassIfShieldUp;

            if (healthComp.shield == null)
                return !PassIfShieldUp;


            return PassIfShieldUp ? healthComp.shield.IsAlive : !healthComp.shield.IsAlive;

        }
    }
}
