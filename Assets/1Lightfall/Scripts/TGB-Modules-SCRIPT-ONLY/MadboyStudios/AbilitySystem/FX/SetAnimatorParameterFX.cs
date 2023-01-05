using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class SetAnimatorParameterFX : AbilityFXBase
    {

        [SerializeField]
        private string parameterName;
        [SerializeField]
        private AnimationParameterType parameterType = AnimationParameterType.Trigger;
        [SerializeField, ShowIf("@parameterType == AnimationParameterType.Float")]
        private float floatValueOnActivation;
        [SerializeField, ShowIf("@parameterType == AnimationParameterType.Float")]
        private float floatValueOnDeactivation;
        [SerializeField, ShowIf("@parameterType == AnimationParameterType.Bool")]
        private bool boolValueOnActivation;
        [SerializeField, ShowIf("@parameterType == AnimationParameterType.Bool")]
        private bool boolValueOnDeactivation;
        [SerializeField, ShowIf("@parameterType == AnimationParameterType.Trigger")]
        private bool triggerAgainOnDeactivation;

        [SerializeField]
        private bool setLayerWeight;
        [SerializeField, ShowIf("setLayerWeight")]
        private int layerIndex = 0;
        [SerializeField, ShowIf("setLayerWeight"), Range(0, 1)]
        private float layerWeightOnActivation = 0;
        [SerializeField, ShowIf("setLayerWeight"), Range(0, 1)]
        private float layerWeightOnDeactivation = 0;

        private Animator animator;
        private int parameterID;

        public override void Activate(AbilityWrapperBase wrapper)
        {
            Startup(wrapper);
            if (animator == null)
                return;

            switch (parameterType)
            {
                case AnimationParameterType.Float:
                    animator.SetFloat(parameterID, floatValueOnActivation);
                    break;

                case AnimationParameterType.Bool:
                    animator.SetBool(parameterID, boolValueOnActivation);
                    break;

                case AnimationParameterType.Trigger:
                    animator.SetTrigger(parameterID);
                    break;
            }

            if (setLayerWeight)
                animator.SetLayerWeight(layerIndex, layerWeightOnActivation);

        }

        public override void Deactivate(AbilityWrapperBase wrapper)
        {
            Startup(wrapper);
            if (animator == null)
                return;

            switch (parameterType)
            {
                case AnimationParameterType.Float:
                    animator.SetFloat(parameterID, floatValueOnDeactivation);
                    break;

                case AnimationParameterType.Bool:
                    animator.SetBool(parameterID, boolValueOnDeactivation);
                    break;

                case AnimationParameterType.Trigger:
                    if (triggerAgainOnDeactivation)
                        animator.SetTrigger(parameterID);
                    break;
            }

            if (setLayerWeight)
                animator.SetLayerWeight(layerIndex, layerWeightOnDeactivation);
        }

        private void Startup(AbilityWrapperBase wrapper)
        {
            if (animator == null)
            {
                animator = wrapper.Origin.GetComponentInChildren<Animator>();
                parameterID = Animator.StringToHash(parameterName);
            }

        }

        enum AnimationParameterType
        {
            Float,
            Bool,
            Trigger,
        }
    }
}
