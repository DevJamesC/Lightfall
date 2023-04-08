using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorParameterHook : MonoBehaviour
{
    public List<AnimatorParameterDetail> ParameterNameDetail;

    private Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public void SetTrigger(string parameterName)
    {
        animator.SetTrigger(parameterName);
    }

    public void SetBool(string parameterName, bool value)
    {
        animator.SetBool(parameterName, value);
    }


    [Serializable]
    public class AnimatorParameterDetail
    {
        public string Name;
        public AnimatorParameterDetailType AnimatorParameterType;

        [Serializable]
        public enum AnimatorParameterDetailType
        {
            Trigger,
            Bool
        }
    }
}
